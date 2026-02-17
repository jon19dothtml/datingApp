using Core.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace MyProject.Tests
{
    public class MemberRepositoryTests
    {
        private readonly DbContextOptions<AppDbContext> _options;
        private readonly AppDbContext _context;
        private readonly MemberRepository _sut;
        public MemberRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) // DB isolato
                .Options;

            _context= new AppDbContext(options);
            _sut = new MemberRepository(_context);
        }

        [Fact]
        public async Task GetCities_WhenCityExist_ShouldReturnCleanList()
        {
            // ARRANGE
            //var options = new DbContextOptionsBuilder<AppDbContext>()
            //    .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) // DB isolato
            //    .Options;
//TO DO
//nome della città deve essere unique. con nuova migration
//

            var testId = "ooo-3333";
            
                // Add required properties DisplayName, Gender, and Country to each Member initializer
            var list = new List<Member>
            {
                new Member { Id = testId, City = "Roma", DisplayName = "Test1", Gender = "M", Country = "Italy" },
                new Member { Id = "2", City = "lamadcqua", DisplayName = "Test2", Gender = "F", Country = "Italy" },   // Duplicato
                new Member { Id = "3", City = "fdfdsfs", DisplayName = "Test3", Gender = "M", Country = "Italy" },       // Vuoto
                //new Member { Id = "4", City = "Bari", DisplayName = "Test4", Gender = "F", Country = "Italy" },     // Null
                new Member { Id = "5", City = "Milano", DisplayName = "Test5", Gender = "M", Country = "Italy" }
            };
            _context.Members.AddRange(list);
            await _context.SaveChangesAsync();
            

            // ACT
            
            //var repository = new MemberRepository(context);
            var result = await _sut.GetCities();   //CONTROLLARE STRINGHE VUOTE E DISTINCT NEL METODO DELLA REPO

            // ASSERT
            Assert.NotNull(result);
            Assert.Equal(4, result.Count); 
            Assert.Contains("Roma", result);
            Assert.Contains("Milano", result);
            //Assert.DoesNotContain("fdfdsfs", result);
            Assert.Single(result, c => c == "Roma");
            
        }
    }
}