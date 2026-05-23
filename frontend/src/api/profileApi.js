const BASE_URL = "http://localhost:5112/api/auth";

async function apiFetch(url, options = {}) {
    return fetch(url, {
        credentials: "include",
        ...options,
        headers: {
            "Content-Type": "application/json",
            ...(options.headers || {})
        }
    });
}

export async function getProfile() {
    const res = await apiFetch(`${BASE_URL}/profile`, {
    methood: "GET",
    credentials: "include"
    });

    if (!res.ok) {
        throw new Error(await res.text());
    }

    return await res.json();
}

