const AUTH_URL = "http://localhost:5112/api/auth";

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
    const res = await authFetch(`${AUTH_URL}/login`, {
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
    const res = await authFetch(`${AUTH_URL}/register`, {
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
    const res = await authFetch(`${AUTH_URL}/me`,{
    credentials : "include"}
    );

    if (!res.ok) {
        return null;
    }

    return await res.json();
}
export async function logout() {

    const res = await fetch(
        `${AUTH_URL}/logout`,
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