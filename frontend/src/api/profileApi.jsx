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
    const res = await apiFetch('/api/auth/profile', {
    methood: "GET",
    credentials: "include"
    });

    if (!res.ok) {
        throw new Error(await res.text());
    }
    const data = await res.json();
    console.log("profile: ", data)

    return data;
}

