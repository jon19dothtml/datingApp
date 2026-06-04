import { HttpEvent, HttpHandlerFn, HttpInterceptorFn, HttpRequest, HttpResponse } from '@angular/common/http';
import { Observable, of, throwError } from 'rxjs';
import { delay } from 'rxjs/operators';

// ─── Fake JWT token: payload = {"role":"Member"} ───────────────────────────
// header: {} | payload: {"role":"Member"} | signature: fake
const FAKE_TOKEN = 'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJyb2xlIjpbIk1lbWJlciIsIkFkbWluIiwiTW9kZXJhdG9yIl0sInN1YiI6InVzZXItMSJ9.mock_sig';

// ─── Mock Users ─────────────────────────────────────────────────────────────
const MOCK_USER = {
  id: 'user-1',
  displayName: 'Marco Rossi',
  email: 'marco.rossi@tennis.it',
  token: FAKE_TOKEN,
  imageUrl: null,
  roles: ['Member'],
};

// ─── Mock Members (Tennis Players) ─────────────────────────────────────────
const MOCK_MEMBERS = [
  {
    id: 'member-1',
    displayName: 'Marco Rossi',
    dateOfBirth: '1990-05-14T00:00:00',
    imageUrl: null,
    created: '2023-01-15T10:00:00',
    lastActive: '2026-06-01T09:30:00',
    gender: 'male',
    description: 'Appassionato di tennis da 8 anni. Gioco principalmente sul fondo campo con un ottimo rovescio bimane. Cerco avversari di livello simile per allenamenti settimanali.',
    city: 'Milano',
    country: 'Italy',
  },
  {
    id: 'member-2',
    displayName: 'Giulia Ferrari',
    dateOfBirth: '1994-08-22T00:00:00',
    imageUrl: null,
    created: '2023-03-10T12:00:00',
    lastActive: '2026-06-03T15:00:00',
    gender: 'female',
    description: 'Tennista avanzata con 10 anni di esperienza. Stile aggressivo da baseline. Disponibile nei weekend per tornei locali e allenamenti.',
    city: 'Roma',
    country: 'Italy',
  },
  {
    id: 'member-3',
    displayName: 'Luca Bianchi',
    dateOfBirth: '1988-11-30T00:00:00',
    imageUrl: null,
    created: '2023-06-01T08:00:00',
    lastActive: '2026-05-28T18:00:00',
    gender: 'male',
    description: 'Principiante entusiasta! Ho iniziato a giocare 2 anni fa. Cerco pazienti partner per praticare e migliorarmi. Disponibile la sera infrasettimanale.',
    city: 'Torino',
    country: 'Italy',
  },
  {
    id: 'member-4',
    displayName: 'Sofia Ricci',
    dateOfBirth: '1997-03-08T00:00:00',
    imageUrl: null,
    created: '2023-07-20T09:00:00',
    lastActive: '2026-06-02T11:00:00',
    gender: 'female',
    description: 'Giocatrice intermedia con ottimo servizio. Preferisco la terra battuta. Cerco partner per doppio misto o singolare femminile.',
    city: 'Napoli',
    country: 'Italy',
  },
  {
    id: 'member-5',
    displayName: 'Alessandro Conti',
    dateOfBirth: '1985-07-19T00:00:00',
    imageUrl: null,
    created: '2022-11-05T14:00:00',
    lastActive: '2026-06-04T07:00:00',
    gender: 'male',
    description: 'Ex-agonista. Gioco a livello semi-professionistico. Disponibile per allenamenti intensivi e partite competitive. Maestro FITP in cerca di sparring partner.',
    city: 'Firenze',
    country: 'Italy',
  },
  {
    id: 'member-6',
    displayName: 'Chiara Martini',
    dateOfBirth: '1999-12-03T00:00:00',
    imageUrl: null,
    created: '2024-01-10T16:00:00',
    lastActive: '2026-06-04T08:30:00',
    gender: 'female',
    description: 'Studentessa universitaria appassionata di tennis. Livello intermedio. Preferisco il cemento. Cerco compagne per sessioni di allenamento divertenti.',
    city: 'Bologna',
    country: 'Italy',
  },
  {
    id: 'member-7',
    displayName: 'Davide Esposito',
    dateOfBirth: '1992-04-25T00:00:00',
    imageUrl: null,
    created: '2023-09-14T11:00:00',
    lastActive: '2026-06-03T20:00:00',
    gender: 'male',
    description: 'Gioco a tennis da 6 anni. Specializzato nel serve & volley. Cerco partner per partite singolo o doppio il sabato mattina.',
    city: 'Palermo',
    country: 'Italy',
  },
  {
    id: 'member-8',
    displayName: 'Martina De Luca',
    dateOfBirth: '1996-09-11T00:00:00',
    imageUrl: null,
    created: '2023-05-22T13:00:00',
    lastActive: '2026-06-01T19:00:00',
    gender: 'female',
    description: 'Amante del tennis e della vita all\'aperto. Livello avanzato. Partecipo a tornei regionali. Cerco partner per doppio o sessioni di allenamento tecnico.',
    city: 'Venezia',
    country: 'Italy',
  },
  {
    id: 'member-9',
    displayName: 'Riccardo Lombardi',
    dateOfBirth: '1991-02-17T00:00:00',
    imageUrl: null,
    created: '2022-08-30T10:00:00',
    lastActive: '2026-06-04T06:45:00',
    gender: 'male',
    description: 'Tennista intermedio con grande passione per il gioco. Cerco avversari per partite amichevoli serali o nei weekend. Tifo Federer!',
    city: 'Verona',
    country: 'Italy',
  },
  {
    id: 'member-10',
    displayName: 'Valentina Russo',
    dateOfBirth: '2000-06-28T00:00:00',
    imageUrl: null,
    created: '2024-02-15T15:00:00',
    lastActive: '2026-06-03T12:00:00',
    gender: 'female',
    description: 'Giovane tennista con 3 anni di esperienza. Amo il gioco aggressivo dalla linea di fondo. Cerco partner per allenamenti e possibilmente tornei doppio.',
    city: 'Genova',
    country: 'Italy',
  },
];

// ─── Mock Messages ───────────────────────────────────────────────────────────
const MOCK_MESSAGES = [
  {
    id: 'msg-1',
    senderId: 'member-2',
    senderDisplayName: 'Giulia Ferrari',
    senderImageUrl: null,
    recipientId: 'user-1',
    recipientDisplayName: 'Marco Rossi',
    recipientImageUrl: null,
    content: 'Ciao Marco! Ti va una partita sabato mattina al Circolo Tennis Milano?',
    dateRead: '2026-06-02T10:00:00',
    messageSent: '2026-06-01T18:30:00',
    currentUserSender: false,
  },
  {
    id: 'msg-2',
    senderId: 'member-5',
    senderDisplayName: 'Alessandro Conti',
    senderImageUrl: null,
    recipientId: 'user-1',
    recipientDisplayName: 'Marco Rossi',
    recipientImageUrl: null,
    content: 'Ottima partita ieri! Rivincita domenica? Possiamo allenarci sul servizio.',
    dateRead: null,
    messageSent: '2026-06-03T09:15:00',
    currentUserSender: false,
  },
  {
    id: 'msg-3',
    senderId: 'user-1',
    senderDisplayName: 'Marco Rossi',
    senderImageUrl: null,
    recipientId: 'member-3',
    recipientDisplayName: 'Luca Bianchi',
    recipientImageUrl: null,
    content: 'Luca, posso aiutarti a migliorare il tuo rovescio! Ci vediamo giovedì sera?',
    dateRead: '2026-06-04T08:00:00',
    messageSent: '2026-06-03T20:00:00',
    currentUserSender: true,
  },
];

// ─── Mock Message Thread ────────────────────────────────────────────────────
const MOCK_THREAD = [
  {
    id: 'th-1',
    senderId: 'member-2',
    senderDisplayName: 'Giulia Ferrari',
    senderImageUrl: null,
    recipientId: 'user-1',
    recipientDisplayName: 'Marco Rossi',
    recipientImageUrl: null,
    content: 'Ciao Marco! Ho visto il tuo profilo. Che livello di gioco hai?',
    dateRead: '2026-06-01T10:05:00',
    messageSent: '2026-06-01T10:00:00',
    currentUserSender: false,
  },
  {
    id: 'th-2',
    senderId: 'user-1',
    senderDisplayName: 'Marco Rossi',
    senderImageUrl: null,
    recipientId: 'member-2',
    recipientDisplayName: 'Giulia Ferrari',
    recipientImageUrl: null,
    content: 'Ciao Giulia! Sono circa NTRP 3.5, gioco da 8 anni. E tu?',
    dateRead: '2026-06-01T10:20:00',
    messageSent: '2026-06-01T10:15:00',
    currentUserSender: true,
  },
  {
    id: 'th-3',
    senderId: 'member-2',
    senderDisplayName: 'Giulia Ferrari',
    senderImageUrl: null,
    recipientId: 'user-1',
    recipientDisplayName: 'Marco Rossi',
    recipientImageUrl: null,
    content: 'Perfetto! Sono 4.0. Sabato mattina al circolo? Potremmo fare un\'ora di allenamento.',
    dateRead: null,
    messageSent: '2026-06-01T10:25:00',
    currentUserSender: false,
  },
];

// ─── Pagination helper ───────────────────────────────────────────────────────
function paginate<T>(items: T[], page = 1, size = 10) {
  const start = (page - 1) * size;
  return {
    items: items.slice(start, start + size),
    metadata: {
      currentPage: page,
      pageSize: size,
      totalCount: items.length,
      totalPages: Math.ceil(items.length / size),
    },
  };
}

function ok(body: any): Observable<HttpEvent<any>> {
  return of(new HttpResponse({ status: 200, body })).pipe(delay(100));
}

// ─── The interceptor ─────────────────────────────────────────────────────────
export const mockInterceptor: HttpInterceptorFn = (
  req: HttpRequest<any>,
  next: HttpHandlerFn
): Observable<HttpEvent<any>> => {
  const url = req.url;
  const method = req.method;

  // i18n translation files → pass through
  if (url.includes('/i18n/')) return next(req);

  // ── Account ──────────────────────────────────────────────────────────────
  if (url.includes('account/refresh-token') && method === 'POST') {
    return ok(MOCK_USER);
  }
  if (url.includes('account/login') && method === 'POST') {
    return ok(MOCK_USER);
  }
  if (url.includes('account/logout') && method === 'POST') {
    return ok({});
  }

  // ── Members ───────────────────────────────────────────────────────────────
  if (url.includes('members/cities')) {
    return ok(['Milano', 'Roma', 'Torino', 'Napoli', 'Firenze', 'Bologna', 'Palermo', 'Venezia', 'Verona', 'Genova']);
  }
  if (url.includes('members/add-photo') && method === 'POST') {
    return ok({ id: 999, url: null, isApproved: true, memberId: 'user-1' });
  }
  if (url.includes('members/set-main-photo') && method === 'PUT') {
    return ok({});
  }
  if (url.includes('members/delete-photo') && method === 'DELETE') {
    return ok({});
  }
  // members/:id  (GET)
  if (/members\/[^/?]+$/.test(url) && method === 'GET') {
    const id = url.split('/').pop();
    const found = MOCK_MEMBERS.find(m => m.id === id) ?? MOCK_MEMBERS[0];
    return ok(found);
  }
  // members/:id  (PUT)
  if (/members\/[^/?]+$/.test(url) && method === 'PUT') {
    return ok({});
  }
  // PUT members (update current user)
  if (/members$/.test(url) && method === 'PUT') {
    return ok({});
  }
  // GET members/:id/photos
  if (url.includes('/photos') && method === 'GET') {
    return ok([]);
  }
  // GET members (list)
  if (url.includes('members') && method === 'GET') {
    const params = req.params;
    const page = parseInt(params.get('pageNumber') ?? '1', 10);
    const size = parseInt(params.get('pageSize') ?? '10', 10);
    return ok(paginate(MOCK_MEMBERS, page, size));
  }

  // ── Likes ─────────────────────────────────────────────────────────────────
  if (url.includes('likes/list')) {
    return ok(['member-2', 'member-5']);
  }
  if (url.includes('likes') && method === 'POST') {
    return ok({});
  }
  if (url.includes('likes')) {
    const params = req.params;
    const page = parseInt(params.get('pageNumber') ?? '1', 10);
    const size = parseInt(params.get('pageSize') ?? '5', 10);
    const liked = MOCK_MEMBERS.filter(m => ['member-2', 'member-5'].includes(m.id));
    return ok(paginate(liked, page, size));
  }

  // ── Messages ──────────────────────────────────────────────────────────────
  if (url.includes('messages/thread')) {
    return ok(MOCK_THREAD);
  }
  if (url.includes('messages') && method === 'DELETE') {
    return ok({});
  }
  if (url.includes('messages') && method === 'POST') {
    return ok({
      id: 'new-msg',
      senderId: 'user-1',
      senderDisplayName: 'Marco Rossi',
      senderImageUrl: null,
      recipientId: 'member-2',
      recipientDisplayName: 'Giulia Ferrari',
      recipientImageUrl: null,
      content: req.body?.content ?? '',
      dateRead: null,
      messageSent: new Date().toISOString(),
      currentUserSender: true,
    });
  }
  if (url.includes('messages')) {
    const params = req.params;
    const page = parseInt(params.get('pageNumber') ?? '1', 10);
    const size = parseInt(params.get('pageSize') ?? '10', 10);
    return ok(paginate(MOCK_MESSAGES, page, size));
  }

  // ── SignalR hubs → let fail silently ─────────────────────────────────────
  if (url.includes('hubs/')) {
    return throwError(() => new Error('SignalR not available in mock mode'));
  }

  // Pass everything else through
  return next(req);
};
