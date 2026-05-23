const circles = document.querySelectorAll(".circle");

circles.forEach(circle => {
    circle.addEventListener("click", async () => {
        const id = circle.dataset.id;

        // Пример запроса к твоему API
        const response = await fetch(`${process.env.REACT_APP_API_URL}/api/tasks`);
        const data = await response.json();

        alert(data.title); // пока просто вывод
    });
});