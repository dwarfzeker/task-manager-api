export async function getTodos(date) {
    const url = `/api/tasks/?data=${date}`;
    console.log('Fetching:', url);  
    
    try {
        const res = await fetch(url, { credentials: 'include' });
        console.log('Response status:', res.status);  
        
        if (!res.ok) {
            console.error('HTTP error:', res.status);
            return [];
        }
        
        const data = await res.json();
        console.log('Raw data:', data);  
        
        
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
    await fetch(`/api/tasks`, {
        method: "POST",
        credentials: "include",
        headers: authHeaders(),
        body: JSON.stringify(todo)
    });
}


export async function updateTodo(id, todo) {
    const res = await fetch(`/api/tasks/${id}`, {
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
    await fetch(`/api/tasks/${id}`, {
        method: "DELETE",
        credentials: "include",
        headers: authHeaders()
    });
}

export async function updatePosition(id, x, y) {
    const res = await fetch(
        `/api/tasks/${id}/position`,
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