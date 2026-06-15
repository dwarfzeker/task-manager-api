async function apiFetch(url, options = {}) {
return fetch(url, {
        credentials: "include",
        cache: "no-cache", // ✅ добавляем это!
        headers: {
            "Content-Type": "application/json",
            "Cache-Control": "no-cache", // ✅ и это
            "Pragma": "no-cache",        // ✅ и это
            ...(options.headers || {})
        },
        ...options
    });
}

export async function getProfile() {
    const res = await apiFetch('/api/auth/profile', {
    method: "GET",
    credentials: "include"
    });

    if (!res.ok) {
        throw new Error(await res.text());
    }
    const data = await res.json();
    console.log("profile: ", data)

    return data;
}

