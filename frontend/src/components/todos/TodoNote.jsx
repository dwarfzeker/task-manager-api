import { useState } from "react";

import {
  updateTodo,
  updatePosition, 
  getTodos
} from "../../api/todoApi";

export default function TodoNote({
  todo,
  index,
  onDelete,
  onEdit,
  onToggleComplete,
  refresh
}) {

  const [dragging, setDragging] = useState(false);

  async function handleMouseDown(e) {

    setDragging(true);

    const note = e.currentTarget;

    const board = note.parentElement;

    const boardRect =
      board.getBoundingClientRect();

    const noteRect =
      note.getBoundingClientRect();

    const offsetX =
      e.clientX - noteRect.left;

    const offsetY =
      e.clientY - noteRect.top;

function onMouseMove(ev) {
    let x = ev.clientX - boardRect.left - offsetX;
    let y = ev.clientY - boardRect.top - offsetY;
    
    // Ограничиваем движение в пределах доски
    x = Math.max(0, Math.min(x, boardRect.width - noteRect.width));
    y = Math.max(0, Math.min(y, boardRect.height - noteRect.height));
    
    // Устанавливаем позицию в ПРОЦЕНТАХ для CSS
    note.style.left = `${(x / boardRect.width) * 100}%`;
    note.style.top = `${(y / boardRect.height) * 100}%`;
}
async function onMouseUp(ev) {
    document.removeEventListener("mousemove", onMouseMove);
    document.removeEventListener("mouseup", onMouseUp);
    
    setDragging(false);
    
    // Вычисляем позицию в пикселях
    let x = ev.clientX - boardRect.left - offsetX;
    let y = ev.clientY - boardRect.top - offsetY;
    
    // Ограничиваем в ПИКСЕЛЯХ
    x = Math.max(0, Math.min(x, boardRect.width - noteRect.width));
    y = Math.max(0, Math.min(y, boardRect.height - noteRect.height));
    
    // Конвертируем в проценты
    const percentX = (x / boardRect.width) * 100;
    const percentY = (y / boardRect.height) * 100;
    
    try {
        await updatePosition(todo.id, percentX, percentY);
        if (refresh) refresh();  // Теперь refresh будет работать
    } catch (error) {
        console.error('Failed to update position:', error);
    }
}

    document.addEventListener(
      "mousemove",
      onMouseMove
    );

    document.addEventListener(
      "mouseup",
      onMouseUp
    );
  }

  return (
    <div
      className={`todo-note ${
        todo.isCompleted ? "completed" : ""
      } color-${index % 9}`}

      onMouseDown={handleMouseDown}

      style={{
        top: `${todo.positionY ?? 20}%`,
        left: `${todo.positionX ?? 20}%`,
        transform:
          `rotate(${(index % 5 - 2) * 4}deg)`
      }}
    >

      <input
        type="checkbox"
        className="todo-checkbox"
        checked={todo.isCompleted}
        onChange={onToggleComplete}
      />

      <h3>{todo.title}</h3>

      {todo.description && (
        <p>{todo.description}</p>
      )}

      <div className="todo-actions">

        <button onClick={onEdit}>
          ✏️
        </button>

        <button onClick={onDelete}>
          ❌
        </button>

      </div>

    </div>
  );
}