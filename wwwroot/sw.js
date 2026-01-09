//Service Worker
// ============================================
// CONFIGURACIÓN Y CONSTANTES
// ============================================
const CACHE_NAME = 'ahorcadito-v3';
const DB_NAME = 'ahorcaditoDB';
const DB_VERSION = 3;

const CACHE_URLS = [
    '/',
    '/index.html',
    '/login.html',
    '/juego.html',
    '/vistaTemas.html',
    '/racha.html',
    '/perfil.html',
    '/notificaciones.html',
    '/scripts/style_temaIndex.css',
    '/scripts/style.css',
    '/scripts/style_vistaTemas.css',
    '/images/ahorcado.png',
    '/images/control.png',
    '/images/Designer (2).png',
    '/images/flama.png',
    '/images/Flecha.png',
    '/images/gaming-game-minecraft-background-music-372242.mp3',
    '/images/hangman-0.svg',
    '/images/hangman-1.svg',
    '/images/hangman-2.svg',
    '/images/hangman-3.svg',
    '/images/hangman-4.svg',
    '/images/hangman-5.svg',
    '/images/hangman-6.svg',
    '/images/hangman-7.svg',
    '/images/lost.gif',
    '/images/lupa.png',
    '/images/mando.png',
    '/images/Notificaciones.png',
    '/images/racha.png',
    '/images/rayo.png',
    '/images/triangulo.png',
    '/images/usuario.png',
    '/images/victory.gif'
];

// ============================================
// INSTALACIÓN DEL SERVICE WORKER
// ============================================
self.addEventListener('install', (event) => {
    event.waitUntil(
        Promise.all([
            caches.open(CACHE_NAME)
                .then(cache => cache.addAll(CACHE_URLS)),
            openDatabase()
        ]).then(() => self.skipWaiting())
    );
});

// ============================================
// ACTIVACIÓN DEL SERVICE WORKER
// ============================================
self.addEventListener('activate', (event) => {
    event.waitUntil(
        caches.keys().then((cacheNames) => {
            return Promise.all(
                cacheNames.map((cacheName) => {
                    if (cacheName !== CACHE_NAME) {
                        return caches.delete(cacheName);
                    }
                })
            );
        }).then(() => self.clients.claim())
    );
});

// ============================================
// INTERCEPTACIÓN DE PETICIONES (FETCH)
// ============================================
self.addEventListener('fetch', (event) => {
    const { request } = event;
    const url = new URL(request.url);

    // Ignorar peticiones que no sean GET
    if (request.method !== 'GET') {
        // Para POST, PUT, DELETE - manejar offline
        event.respondWith(handleWriteRequest(request));
        return;
    }

    // Peticiones a la API
    if (url.pathname.startsWith('/api/')) {
        event.respondWith(handleApiRequest(request));
    }
    // Archivos estáticos
    else {
        event.respondWith(handleStaticRequest(request));
    }
});

// ============================================
// ESTRATEGIA: NETWORK FIRST (API) - MEJORADO PARA OFFLINE
// ============================================
async function handleApiRequest(request) {
    const url = new URL(request.url);

    //Si la petición tiene cache: 'no-cache' Y hay conexión, ir directo a la red
    if (request.cache === 'no-cache' || request.headers.get('Cache-Control') === 'no-cache') {
        try {
            const response = await fetch(request);

            //GUARDAR EN INDEXEDDB PARA USO OFFLINE FUTURO
            if (response.ok && url.pathname.match(/\/api\/Temas\/\d+$/)) {
                const data = await response.clone().json();
                await saveToIndexedDB('temas-api-cache', url.pathname, data);
            }

            return response;
        } catch (error) {

            //SI FALLA, BUSCAR EN INDEXEDDB
            if (url.pathname.match(/\/api\/Temas\/\d+$/)) {
                const cachedData = await getFromIndexedDB('temas-api-cache', url.pathname);
                if (cachedData) {
                    return new Response(JSON.stringify(cachedData), {
                        status: 200,
                        headers: { 'Content-Type': 'application/json' }
                    });
                }
            }

            return new Response(JSON.stringify({ error: 'Sin conexión' }), {
                status: 503,
                headers: { 'Content-Type': 'application/json' }
            });
        }
    }

    try {
        // Intentar obtener de la red
        const networkResponse = await fetch(request);

        // Si es exitoso, guardar en caché e IndexedDB
        if (networkResponse.ok) {
            const cache = await caches.open(CACHE_NAME);
            cache.put(request, networkResponse.clone());

            // Guardar datos importantes en IndexedDB
            if (url.pathname.includes('/api/Temas/pagina/')) {
                const data = await networkResponse.clone().json();
                await saveToIndexedDB('api-cache', request.url, data);
            }

            // GUARDAR TEMAS INDIVIDUALES
            if (url.pathname.match(/\/api\/Temas\/\d+$/)) {
                const data = await networkResponse.clone().json();
                await saveToIndexedDB('temas-api-cache', url.pathname, data);
            }
        }

        return networkResponse;
    } catch (error) {

        // BUSCAR TEMA INDIVIDUAL EN INDEXEDDB
        if (url.pathname.match(/\/api\/Temas\/\d+$/)) {
            const cachedData = await getFromIndexedDB('temas-api-cache', url.pathname);
            if (cachedData) {
                return new Response(JSON.stringify(cachedData), {
                    status: 200,
                    headers: { 'Content-Type': 'application/json' }
                });
            }
        }

        // Intentar obtener de IndexedDB (para otros endpoints)
        const cachedData = await getFromIndexedDB('api-cache', request.url);
        if (cachedData) {
            return new Response(JSON.stringify(cachedData), {
                headers: { 'Content-Type': 'application/json' }
            });
        }

        // Si no, intentar obtener del caché
        const cachedResponse = await caches.match(request);
        if (cachedResponse) {
            return cachedResponse;
        }

        // Respuesta de error
        return new Response(JSON.stringify({
            error: 'Sin conexión',
            offline: true
        }), {
            status: 503,
            headers: { 'Content-Type': 'application/json' }
        });
    }
}

// ============================================
// ESTRATEGIA: CACHE FIRST (ESTÁTICOS)
// ============================================
async function handleStaticRequest(request) {
    // Primero buscar en caché
    const cachedResponse = await caches.match(request);
    if (cachedResponse) {
        return cachedResponse;
    }

    // Si no está, obtener de la red
    try {
        const networkResponse = await fetch(request);
        if (networkResponse.ok) {
            const cache = await caches.open(CACHE_NAME);
            cache.put(request, networkResponse.clone());
        }
        return networkResponse;
    } catch (error) {
        // Si falla, devolver página offline personalizada
        if (request.destination === 'document') {
            return caches.match('/index.html');
        }
        return new Response('Sin conexión', { status: 503 });
    }
}

// ============================================
// MANEJO DE PETICIONES DE ESCRITURA (POST/PUT/DELETE)
// ============================================
async function handleWriteRequest(request) {
    const clonedRequest = request.clone(); // Clonar ANTES de usar

    try {
        // Intentar enviar la petición
        const response = await fetch(request);
        return response;
    } catch (error) {

        // Si falla, guardar en IndexedDB para sincronizar después
        let body = null;

        if (clonedRequest.method !== 'DELETE' && clonedRequest.method !== 'GET') {
            try {
                body = await clonedRequest.text();
            } catch (e) {
            }
        }

        const requestData = {
            id: `${clonedRequest.url}-${Date.now()}`, // ID único
            url: clonedRequest.url,
            method: clonedRequest.method,
            headers: Array.from(clonedRequest.headers.entries()),
            body: body,
            timestamp: Date.now(),
            type: clonedRequest.url.includes('/reiniciar-progreso') ? 'reinicio-progreso' : undefined
        };

        await saveToIndexedDB('pending-requests', requestData.id, requestData);


        // Registrar sync para cuando haya conexión
        try {
            if (self.registration.sync) {
                await self.registration.sync.register('sync-data');
            }
        } catch (syncError) {
        }

        // Devolver respuesta simulada de éxito
        return new Response(JSON.stringify({
            success: true,
            offline: true,
            message: 'Se guardará cuando haya conexión'
        }), {
            status: 200,
            headers: { 'Content-Type': 'application/json' }
        });
    }
}

// ============================================
// BACKGROUND SYNC - MEJORADO
// ============================================
self.addEventListener('sync', (event) => {
    if (event.tag === 'sync-data') {
        event.waitUntil(syncPendingRequests());
    }
});

async function syncPendingRequests() {
    try {
        const pendingRequests = await getAllFromIndexedDB('pending-requests');
        if (pendingRequests.length === 0) {
            return;
        }

        let sincronizadas = 0;
        let fallidas = 0;

        for (const reqData of pendingRequests) {
            try {
                const options = {
                    method: reqData.method,
                    headers: new Headers(reqData.headers)
                };

                if (reqData.body && reqData.method !== 'DELETE' && reqData.method !== 'GET') {
                    options.body = reqData.body;
                }

                const response = await fetch(reqData.url, options);

                if (response.ok) {
                    // Eliminar de IndexedDB
                    await deleteFromIndexedDB('pending-requests', reqData.id);
                    sincronizadas++;
                    // Notificar a los clientes
                    const clients = await self.clients.matchAll();
                    clients.forEach(client => {
                        client.postMessage({
                            type: 'SYNC_COMPLETE',
                            url: reqData.url,
                            method: reqData.method
                        });
                    });
                } else {
                    fallidas++;
                }
            } catch (error) {
                fallidas++;
            }
        }
        // Notificar resultado final
        const clients = await self.clients.matchAll();
        clients.forEach(client => {
            client.postMessage({
                type: 'SYNC_RESULTADO',
                sincronizadas: sincronizadas,
                fallidas: fallidas
            });
        });

    } catch (error) {
    }
}

// ============================================
// FUNCIONES DE INDEXEDDB - MEJORADAS
// ============================================
function openDatabase() {
    return new Promise((resolve, reject) => {
        const request = indexedDB.open(DB_NAME, DB_VERSION);

        request.onerror = () => reject(request.error);
        request.onsuccess = () => resolve(request.result);

        request.onupgradeneeded = (event) => {
            const db = event.target.result;

            // Store para caché de API
            if (!db.objectStoreNames.contains('api-cache')) {
                db.createObjectStore('api-cache', { keyPath: 'url' });
            }

            // NUEVO STORE PARA TEMAS INDIVIDUALES
            if (!db.objectStoreNames.contains('temas-api-cache')) {
                db.createObjectStore('temas-api-cache', { keyPath: 'path' });
            }

            // Store para peticiones pendientes
            if (!db.objectStoreNames.contains('pending-requests')) {
                db.createObjectStore('pending-requests', { keyPath: 'id' });
            }

            // Store para temas descargados
            if (!db.objectStoreNames.contains('temas')) {
                db.createObjectStore('temas', { keyPath: 'idTema' });
            }

            // Store para progreso local
            if (!db.objectStoreNames.contains('progreso-local')) {
                db.createObjectStore('progreso-local', { keyPath: 'idTema' });
            }

            // Store para progreso offline
            if (!db.objectStoreNames.contains('progreso-offline')) {
                db.createObjectStore('progreso-offline', { autoIncrement: true });
            }

            
            if (!db.objectStoreNames.contains('estadisticas-cache')) {
                db.createObjectStore('estadisticas-cache', { keyPath: 'id' });
            }

            if (!db.objectStoreNames.contains('perfil-cache')) {
                db.createObjectStore('perfil-cache', { keyPath: 'id' });
            }

            
            if (!db.objectStoreNames.contains('racha-cache')) {
                db.createObjectStore('racha-cache', { keyPath: 'id' });
            }

            if (!db.objectStoreNames.contains('notificaciones-cache')) {
                db.createObjectStore('notificaciones-cache', { keyPath: 'id' });
            }
        };
    });
}

// ============================================
// saveToIndexedDB
// ============================================
async function saveToIndexedDB(storeName, key, value) {
    try {
        const db = await openDatabase();
        const transaction = db.transaction([storeName], 'readwrite');
        const store = transaction.objectStore(storeName);

        let data;
        if (storeName === 'api-cache') {
            data = { url: key, data: value, timestamp: Date.now() };
        } else if (storeName === 'temas-api-cache') {
            // GUARDAR TEMAS INDIVIDUALES CON PATH COMO KEY
            data = { path: key, data: value, timestamp: Date.now() };
        } else if (storeName === 'pending-requests') {
            data = value; // Ya tiene ID en el objeto
        } else {
            data = value;
        }

        return new Promise((resolve, reject) => {
            const request = store.put(data);
            request.onsuccess = () => resolve(request.result);
            request.onerror = () => {
                reject(request.error);
            };
        });
    } catch (error) {
        throw error;
    }
}

async function getFromIndexedDB(storeName, key) {
    try {
        const db = await openDatabase();
        const transaction = db.transaction([storeName], 'readonly');
        const store = transaction.objectStore(storeName);

        return new Promise((resolve, reject) => {
            const request = store.get(key);
            request.onsuccess = () => {
                const result = request.result;
                if (result) {
                    if (storeName === 'api-cache' || storeName === 'temas-api-cache') {
                        // Verificar si los datos no son muy antiguos (7 días para temas)
                        const maxAge = storeName === 'temas-api-cache'
                            ? 7 * 24 * 60 * 60 * 1000  // 7 días
                            : 24 * 60 * 60 * 1000;      // 24 horas
                            
                        const age = Date.now() - result.timestamp;
                        if (age < maxAge) {
                            resolve(result.data);
                        } else {
                            resolve(null);
                        }
                    } else {
                        resolve(result);
                    }
                } else {
                    resolve(null);
                }
            };
            request.onerror = () => reject(request.error);
        });
    } catch (error) {
        return null;
    }
}

async function getAllFromIndexedDB(storeName) {
    try {
        const db = await openDatabase();
        const transaction = db.transaction([storeName], 'readonly');
        const store = transaction.objectStore(storeName);

        return new Promise((resolve, reject) => {
            const request = store.getAll();
            request.onsuccess = () => resolve(request.result || []);
            request.onerror = () => reject(request.error);
        });
    } catch (error) {
        return [];
    }
}

async function deleteFromIndexedDB(storeName, key) {
    try {
        const db = await openDatabase();
        const transaction = db.transaction([storeName], 'readwrite');
        const store = transaction.objectStore(storeName);

        return new Promise((resolve, reject) => {
            const request = store.delete(key);
            request.onsuccess = () => {
                resolve();
            };
            request.onerror = () => {
                reject(request.error);
            };
        });
    } catch (error) {
    }
}

// ============================================
// NOTIFICACIONES PUSH - SIEMPRE MOSTRAR
// ============================================
self.addEventListener('push', (event) => {
    event.waitUntil(mostrarNotificacion(event));
});

async function mostrarNotificacion(event) {
    //en data viene el json que mandamos desde el service
    if (!event.data) return;

    try {
        const data = event.data.json();
        //buscar las ventanas (o pestañas) que registraron el service worker
        const windows = await clients.matchAll({ type: 'window' });
        //verificar si alguna esta actualmente visible
        const appVisible = windows.some(w => w.visibilityState === 'visible');

        // SIEMPRE MOSTRAR NOTIFICACIÓN DEL SISTEMA
        await self.registration.showNotification(data.titulo, {
            body: data.mensaje,
            icon: data.icono || '/images/android/android-launchericon-192-192.png',
            badge: data.badge || '/images/badge-72.png',
            vibrate: [200, 100, 200],
            tag: 'ahorcadito-notification',
            requireInteraction: false,
            silent: false,
            data: {
                url: data.url || '/'
            }
        });

        if (appVisible) {
            // Enviar mensaje a las ventanas visibles, no saldra la ventanita
            //de notificaciones
            // Si la app está visible, enviar mensaje interno
            for (let w of windows) {
                if (w.visibilityState === 'visible') {
                    w.postMessage({
                        type: 'NOTIFICACION_RECIBIDA',
                        titulo: data.titulo,
                        mensaje: data.mensaje
                    });
                }
            }
        }
    } catch (error) {
    }
}

self.addEventListener('notificationclick', (event) => {
    event.notification.close();

    const urlToOpen = event.notification.data?.url || '/';

    event.waitUntil(
        clients.matchAll({ type: 'window', includeUncontrolled: true })
            .then((clientList) => {
                // Si ya hay una ventana abierta con esa URL, enfocarla
                for (let client of clientList) {
                    if (client.url === urlToOpen && 'focus' in client) {
                        return client.focus();
                    }
                }
                // Si no, abrir nueva ventana
                if (clients.openWindow) {
                    return clients.openWindow(urlToOpen);
                }
            })
    );
});