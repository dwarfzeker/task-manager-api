async function authFetch(url, options = {}) {
    return fetch(url, {
        credentials: "include",
        ...options,
        headers: {
            "Content-Type": "application/json",
            ...(options.headers || {})
        }
    });
}

export async function login(data) {
    const res = await authFetch('/api/auth/login' , {
        method: "POST",
        credentials: "include",
        body: JSON.stringify(data)
    });

    if (!res.ok) {
        throw new Error(await res.text());
    }

    return await res.json();
}

export async function register(data) {
    const res = await authFetch('/api/auth/register', {
        method: "POST",
        credentials: "include",
        body: JSON.stringify(data)
    });

    if (!res.ok) {
        throw new Error(await res.text());
    }

    return await res.json();
}

export async function getMe() {
    const res = await authFetch('/api/auth/me', {
    credentials : "include"});

    if (!res.ok) {
        return null;
    }

    return await res.json();
}
export async function logout() {

    const res = await fetch(
        '/api/auth/logout',
        {
            method: "POST",

            credentials: "include"
        }
    );

    if (!res.ok) {
        throw new Error(await res.text());
    }

    return await res.text();
}