const cacheName = "DefaultCompany-s1-2.0";
const contentToCache = [
    "Build/WebGL_Build.loader.js",
    "Build/WebGL_Build.framework.js",
    "Build/WebGL_Build.data",
    "Build/WebGL_Build.wasm",
    "TemplateData/style.css"
];

self.addEventListener('install', function (e) {
    console.log('[Service Worker] Install v2.0');
    
    e.waitUntil((async function () {
        try {
            const cache = await caches.open(cacheName);
            console.log('[Service Worker] Caching all: app shell and content');
            await cache.addAll(contentToCache);
        } catch (error) {
            console.error('[Service Worker] Cache installation failed:', error);
        }
    })());
    
    // Force activation of new service worker
    self.skipWaiting();
});

self.addEventListener('activate', function(e) {
    console.log('[Service Worker] Activate v2.0');
    
    e.waitUntil((async function() {
        // Delete old caches
        const cacheNames = await caches.keys();
        await Promise.all(
            cacheNames.map(name => {
                if (name !== cacheName) {
                    console.log('[Service Worker] Deleting old cache:', name);
                    return caches.delete(name);
                }
            })
        );
    })());
    
    // Take control of all clients immediately
    return self.clients.claim();
});

self.addEventListener('fetch', function (e) {
    const request = e.request;
    
    // Skip non-http requests
    if (!request.url.startsWith('http')) {
        return;
    }
    
    // Skip browser extension requests
    if (request.url.startsWith('chrome-extension:') ||
        request.url.startsWith('moz-extension:') ||
        request.url.startsWith('webkit-extension:')) {
        return;
    }
    
    // CRITICAL: Only handle GET requests - everything else goes through
    if (request.method !== 'GET') {
        console.log('[Service Worker] Bypassing non-GET request:', request.method, request.url);
        return;
    }
    
    // Skip blob and data URLs
    if (request.url.startsWith('blob:') || request.url.startsWith('data:')) {
        console.log('[Service Worker] Bypassing blob/data request:', request.url);
        return;
    }
    
    // Handle GET requests with caching
    e.respondWith(handleGetRequest(request));
});

async function handleGetRequest(request) {
    try {
        // Check cache first
        const cachedResponse = await caches.match(request);
        if (cachedResponse) {
            console.log('[Service Worker] Cache hit:', request.url);
            return cachedResponse;
        }
        
        // Fetch from network
        console.log('[Service Worker] Fetching from network:', request.url);
        const networkResponse = await fetch(request);
        
        // Only cache successful GET responses
        if (networkResponse && 
            networkResponse.status === 200 && 
            networkResponse.type === 'basic' &&
            request.method === 'GET') {
            
            try {
                const cache = await caches.open(cacheName);
                console.log('[Service Worker] Caching response:', request.url);
                await cache.put(request, networkResponse.clone());
            } catch (cacheError) {
                console.warn('[Service Worker] Failed to cache:', request.url, cacheError);
            }
        }
        
        return networkResponse;
        
    } catch (error) {
        console.error('[Service Worker] Fetch failed:', request.url, error);
        
        // Fallback: try to fetch directly without caching
        try {
            return await fetch(request);
        } catch (fallbackError) {
            console.error('[Service Worker] Fallback fetch failed:', request.url, fallbackError);
            throw fallbackError;
        }
    }
}
