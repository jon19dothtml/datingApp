using Core.Entities;
using Core.Helpers;
using FakeItEasy;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using MockQueryable;
using MockQueryable.FakeItEasy;
using System.Diagnostics.Metrics;
using System.Reflection;
using Xunit;

namespace MyProject.Tests
{
    //MemberRepositoryTests
    //to do creare un oggetto fake con fake it easy, creando poi un istanza di memberRepository passandoli il fake db context

    public class MemberRepositoryTests
    {
        private readonly AppDbContext _fakeContext;
        private readonly MemberRepository _sut;
        private readonly DbSet<Member> _fakeDbSet;

        public MemberRepositoryTests()
        {
            // Creiamo delle opzioni reali (ma vuote) da passare al costruttore del fake
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: "FakeDb")
                .Options;

            // Specifichiamo a FakeItEasy di usare quelle opzioni
            _fakeContext = A.Fake<AppDbContext>(x => x.WithArgumentsForConstructor(new object[] { options }));

            _fakeDbSet = A.Fake<DbSet<Member>>(d => d.Implements<IQueryable<Member>>());
            _sut = new MemberRepository(_fakeContext);
        }


        //TEST SU GETCITIES()
        [Fact]
        public async Task GetCities_ShouldReturnExpectedList()
        {
            // 1. ARRANGE
            var data = new List<Member>
            {
                new Member { Id = "1", City = "Roma", DisplayName = "User1", Gender="Male", Country="Italy" },
                new Member { Id = "2", City = "", DisplayName = "User2", Gender="Male", Country="Italy" }
            }.BuildMock();

            //A cosa serve BuildMock: BuildMock() non ti ritorna un semplice IQueryable, ma un oggetto MOLTO più complesso, già completo di tutto ciò che serve a EF Core per funzionare
            //come un DbSet vero. Implementa IAsyncEnumerable<T> IAsyncQueryProvider GetAsyncEnumerator ed è automaticamente compatibile con ToListAsync()

            A.CallTo(() => ((IQueryable<Member>)_fakeDbSet).Provider).Returns(data.Provider); // serve a eseguire l’espressione LINQ (Select, Where, ecc.)
            A.CallTo(() => ((IQueryable<Member>)_fakeDbSet).Expression).Returns(data.Expression); //Expression è un albero di espressioni, che serve ad EF per costruire la query
            A.CallTo(() => ((IQueryable<Member>)_fakeDbSet).ElementType).Returns(data.ElementType); //Serve a LINQ per sapere che tipo di elementi contiene la query. In questo caso: Member.
            A.CallTo(() => ((IQueryable<Member>)_fakeDbSet).GetEnumerator()).Returns(data.GetEnumerator()); //Serve a LINQ per "scorrere" gli elementi della query (nei forach, ToList ecc.)

            A.CallTo(() => _fakeContext.Members).Returns(_fakeDbSet); //colleghiamo il finto dbContext con il finto dbSet

            // 2. ACT

            var result = await _sut.GetCities();

            // 3. ASSERT
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Contains("Roma", result);
            Assert.Contains("Milano", result);
            Assert.DoesNotContain("", result);
        }

        //TEST SU GETCOUNTRIES()
        [Fact]
        public async Task GetCountries_ShouldReturnExpectedList()
        {
            // 1. ARRANGE
            var data = new List<Member>
            {
                new Member { Id = "1", City = "Roma", DisplayName = "User1", Gender="Male", Country="Italy" },
                new Member { Id = "2", City = "Milano", DisplayName = "User2", Gender="Male", Country="France" },
                new Member { Id = "3", City = "City3", DisplayName = "User3", Gender="Female", Country="USA" },
                new Member { Id = "3", City = "City4", DisplayName = "User4", Gender="Female", Country="Spain" }
            }.BuildMock();

            A.CallTo(() => ((IQueryable<Member>)_fakeDbSet).Provider).Returns(data.Provider);
            A.CallTo(() => ((IQueryable<Member>)_fakeDbSet).Expression).Returns(data.Expression);
            A.CallTo(() => ((IQueryable<Member>)_fakeDbSet).ElementType).Returns(data.ElementType);
            A.CallTo(() => ((IQueryable<Member>)_fakeDbSet).GetEnumerator()).Returns(data.GetEnumerator());
            A.CallTo(() => _fakeContext.Members).Returns(_fakeDbSet);

            //ACT
            var result = await _sut.GetCountries();

            //ASSERT
            Assert.NotNull(result);
            Assert.Equal(4, result.Count);
            Assert.DoesNotContain("", result);
            Assert.Distinct(result);
        }

        [Fact]
        public async Task GetMemberByIdAsync_ShouldReturnMember_WhenFound()
        {
            // Arrange
            var expected = new Member { Id = "123", City = "Roma", DisplayName = "User1", Gender = "Male", Country = "Italy" };

            // Mock di FindAsync
            A.CallTo(() => _fakeDbSet.FindAsync("123")).Returns(ValueTask.FromResult<Member?>(expected));

            A.CallTo(() => _fakeContext.Members).Returns(_fakeDbSet);

            //ACT
            var result = await _sut.GetMemberByIdAsync("123");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("123", result.Id);

            A.CallTo(() => _fakeDbSet.FindAsync("123")) //verifica che FindAsync sia stato effettivamente chiamato una sola volta durante l'esecuzione di questo metodo
                .MustHaveHappenedOnceExactly(); // in caso di modifiche future, se il findAsync è stato eliminato lui se ne accorge
        }

        [Fact]
        public async Task GetMemberByIdAsync_ShouldReturnNull_WhenNotFound()
        {
            //ARRANGE
            A.CallTo(() => _fakeDbSet.FindAsync("xxx")).Returns(ValueTask.FromResult<Member?>(null));

            A.CallTo(() => _fakeContext.Members).Returns(_fakeDbSet);

            //ACT
            var result = await _sut.GetMemberByIdAsync("xxx");

            //ASSERT
            Assert.Null(result);

            A.CallTo(() => _fakeDbSet.FindAsync("xxx"))
                .MustHaveHappenedOnceExactly();
        }


        [Fact]
        public async Task GetMembersAsync_ShouldFilter_OrderByLastActive()
        {
            // ---------- ARRANGE ----------
            // Oggi per il calcolo età
            var today = DateOnly.FromDateTime(DateTime.Today);

            // Helper per creare DOB con età desiderata
            DateOnly Dob(int years) => DateOnly.FromDateTime(DateTime.Today.AddYears(-years));

            var data = new List<Member>
            {
                // Va escluso (perchè sono il CurrentMember)
                new Member { Id = "me", Gender = "Female", City="Bari", Country="Italy", DisplayName="User1",
                             DateOfBirth = Dob(25), LastActive = DateTime.UtcNow.AddDays(-1), Created = DateTime.UtcNow.AddDays(-10) },

                // Candidati utili: Female, Bari, Italy, età nel range 20-30
                new Member { Id = "1", Gender = "Female", City="Bari", Country="italy", DisplayName="User2",
                             DateOfBirth = Dob(22), LastActive = DateTime.UtcNow.AddDays(-2), Created = DateTime.UtcNow.AddDays(-5) },

                new Member { Id = "2", Gender = "Female", City="Bari", Country="ITALY", DisplayName="User3",
                             DateOfBirth = Dob(28), LastActive = DateTime.UtcNow.AddDays(-1), Created = DateTime.UtcNow.AddDays(-3) },

                // Fuori gender
                new Member { Id = "3", Gender = "Male", City="Bari", Country="Italy", DisplayName="User4",
                             DateOfBirth = Dob(26), LastActive = DateTime.UtcNow.AddDays(-1), Created = DateTime.UtcNow.AddDays(-2) },

                // Fuori city
                new Member { Id = "4", Gender = "Female", City="Roma", Country="Italy", DisplayName="User5",
                             DateOfBirth = Dob(24), LastActive = DateTime.UtcNow.AddDays(-4), Created = DateTime.UtcNow.AddDays(-2) },

                // Fuori country (trim/case-insensitive verifica)
                new Member { Id = "5", Gender = "Female", City="Bari", Country="France", DisplayName="User6",
                             DateOfBirth = Dob(23), LastActive = DateTime.UtcNow.AddDays(-3), Created = DateTime.UtcNow.AddDays(-1) },

                // Fuori età (troppo giovane)
                new Member { Id = "6", Gender = "Female", City="Bari", Country="Italy", DisplayName="User7",
                             DateOfBirth = Dob(18), LastActive = DateTime.UtcNow.AddDays(-6), Created = DateTime.UtcNow.AddDays(-6) },

                // Fuori età (troppo “vecchia” per il range)
                new Member { Id = "7", Gender = "Female", City="Bari", Country="Italy", DisplayName="User8",
                             DateOfBirth = Dob(35), LastActive = DateTime.UtcNow.AddDays(-7), Created = DateTime.UtcNow.AddDays(-7) },
            }.BuildMock();

            // Wiring IQueryable sul fake DbSet
            A.CallTo(() => ((IQueryable<Member>)_fakeDbSet).Provider).Returns(data.Provider);
            A.CallTo(() => ((IQueryable<Member>)_fakeDbSet).Expression).Returns(data.Expression);
            A.CallTo(() => ((IQueryable<Member>)_fakeDbSet).ElementType).Returns(data.ElementType);
            A.CallTo(() => ((IQueryable<Member>)_fakeDbSet).GetEnumerator()).Returns(data.GetEnumerator());

            // DbContext.Members -> fake DbSet
            A.CallTo(() => _fakeContext.Members).Returns(_fakeDbSet);

            var memberParams = new MemberParams
            {
                CurrentMemberId = "me",
                Gender = "Female",
                MinAge = 20,
                MaxAge = 30,
                OrderBy = "lastActiveShouldTriggerDefault", // qualsiasi valore != "created" usa LastActive desc
                City = "Bari",
                Country = "italy",
                PageNumber = 1,
                PageSize = 2
            };

            // ---------- ACT ----------
            var page = await _sut.GetMembersAsync(memberParams);

            // ---------- ASSERT ----------
            Assert.NotNull(page);
            // Verifica che l’ordinamento di default sia per LastActive desc (quindi prima Id="2" poi Id="1")
            Assert.Collection(page.Items,
                first => Assert.Equal("2", first.Id),
                second => Assert.Equal("1", second.Id)
            );

            // Verifiche puntuali sui filtri
            Assert.All(page.Items, m =>
            {
                Assert.NotEqual("me", m.Id);                        // escluso current
                Assert.Equal("Female", m.Gender);                   // gender
                Assert.Equal("Bari", m.City);                       // city
                Assert.Equal("ITALY", m.Country, ignoreCase: true); // country (case-insensitive)
                                                                    // età nel range 20..30
                var age = today.Year - m.DateOfBirth.Year - (today < new DateOnly(today.Year, m.DateOfBirth.Month, m.DateOfBirth.Day) ? 1 : 0);
                Assert.InRange(age, 20, 30);
            });
        }

        [Fact]
        public async Task GetMemberForUpdate_ShouldReturnMember_WhenIdExists()
        {
            // ARRANGE
            var user = new AppUser { Id = "u1", DisplayName = "TestUser" };
            var photos = new List<Photo>
            {
                new Photo { Id = 1, Url = "test1.jpg" },
                new Photo { Id = 2, Url = "test2.jpg" }
            };

            var data = new List<Member>
            {
                new Member
                {
                    Id = "123",
                    DisplayName = "User123",
                    User = user,
                    Photos = photos,
                    Gender= "Male",
                    City="Bari",
                    Country="Italy"
                },
                new Member
                {
                    Id = "999",
                    DisplayName = "AnotherUser",
                    Gender= "Male",
                    City="Bari",
                    Country="Italy"
                }
            }.BuildMock();

            // Connessione IQueryable → fake DbSet
            A.CallTo(() => ((IQueryable<Member>)_fakeDbSet).Provider).Returns(data.Provider);
            A.CallTo(() => ((IQueryable<Member>)_fakeDbSet).Expression).Returns(data.Expression);
            A.CallTo(() => ((IQueryable<Member>)_fakeDbSet).ElementType).Returns(data.ElementType);
            A.CallTo(() => ((IQueryable<Member>)_fakeDbSet).GetEnumerator()).Returns(data.GetEnumerator());

            // DbContext.Members → fake DbSet
            A.CallTo(() => _fakeContext.Members).Returns(_fakeDbSet);

            // ACT
            var result = await _sut.GetMemberForUpdate("123");

            // ASSERT
            Assert.NotNull(result);
            Assert.Equal("123", result.Id);
            Assert.NotNull(result.User);
            Assert.Equal("TestUser", result.User.DisplayName);
            Assert.NotNull(result.Photos);
            Assert.Equal(2, result.Photos.Count);
        }

        [Fact]
        public async Task GetPhotosByMemberIdAsync_ShouldReturnPhotos_ForGivenMember_WhenIsCurrentUserFalse()
        {
            var memberId = "u1";
            var myPhotos = new List<Photo>
            {
                new Photo {Id=1, Url= "u1_1.png"},
                new Photo {Id=2, Url= "u2_2.jpg"}
            };

            var data = new List<Member>
            {
                new Member {Id=memberId, Photos=myPhotos, Gender = "Female", City="Bari", Country="France", DisplayName="User6"},
                new Member {Id= "u2", Gender = "Female", City="Bari", Country="France", DisplayName="User6",
                    Photos= new List<Photo> {
                        new Photo {Id=3, Url= "u3_3.png"}
                    }}
            }.BuildMock();

            A.CallTo(() => ((IQueryable<Member>)_fakeDbSet).Provider).Returns(data.Provider);
            A.CallTo(() => ((IQueryable<Member>)_fakeDbSet).Expression).Returns(data.Expression);
            A.CallTo(() => ((IQueryable<Member>)_fakeDbSet).ElementType).Returns(data.ElementType);
            A.CallTo(() => ((IQueryable<Member>)_fakeDbSet).GetEnumerator()).Returns(data.GetEnumerator());

            // DbContext.Members → fake DbSet
            A.CallTo(() => _fakeContext.Members).Returns(_fakeDbSet);

            //ACT
            var result = await _sut.GetPhotosByMemberIdAsync(memberId, isCurrentUser: false);

            //ASSERT
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.All(result, p => Assert.StartsWith("u1_", p.Url)); // vengono solo le foto del membro u1

        }
    }
}