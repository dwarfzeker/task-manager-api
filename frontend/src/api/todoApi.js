const BASE_URL = "http://localhost:5112/api/tasks";

export async function getTodos(date) {
    const url = date ? `${BASE_URL}?date=${date}` : BASE_URL;
    console.log('Fetching:', url);  // ← отладка
    
    try {
        const res = await fetch(url, { credentials: 'include' });
        console.log('Response status:', res.status);  // ← отладка
        
        if (!res.ok) {
            console.error('HTTP error:', res.status);
            return [];
        }
        
        const data = await res.json();
        console.log('Raw data:', data);  // ← отладка
        
        // Нормализация: если пришёл объект с items
        if (data && Array.isArray(data.items)) {
            return data.items;
        }
        
        return Array.isArray(data) ? data : [];
    } catch (error) {
        console.error('Fetch error:', error);
        return [];
    }
}

export async function createTodo(todo) {
    await fetch(BASE_URL, {
        method: "POST",
        credentials: "include",
        headers: authHeaders(),
        body: JSON.stringify(todo)
    });
}


export async function updateTodo(id, todo) {
    const res = await fetch(`${BASE_URL}/${id}`, {
        method: "PUT",
        credentials: "include",
        headers: authHeaders(),
        body: JSON.stringify(todo)
    });
    
    if (!res.ok) {
        throw new Error('Failed to update task');
    }
    
    return await res.json();
}
export async function deleteTodo(id) {
    await fetch(`${BASE_URL}/${id}`, {
        method: "DELETE",
        credentials: "include",
        headers: authHeaders()
    });
}

export async function updatePosition(id, x, y) {
    const res = await fetch(
        `${BASE_URL}/${id}/position`,
        {
            method: "PATCH",

            credentials: "include",

            headers: {
                "Content-Type": "application/json"
            },

            body: JSON.stringify({
                id,
                positionX: x,
                positionY: y
            })
        }
    );

    if (!res.ok) {
        throw new Error(await res.text());
    }

    return await res.json();
}
function authHeaders() {
	console.log('All localStorage:', {...localStorage});
    return {
        "Content-Type": "application/json"
    };
}