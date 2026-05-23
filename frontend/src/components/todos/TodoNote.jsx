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

      let x =
        ev.clientX -
        boardRect.left -
        offsetX;

      let y =
        ev.clientY -
        boardRect.top -
        offsetY;

      const maxX =
        boardRect.width - noteRect.width;

      const maxY =
        boardRect.height - noteRect.height;

	x = Math.max(
  		0,
  		Math.min(x, maxX - 20)
	);

	y = Math.max(
  		0,
  		Math.min(y, maxY - 20)
	);

      note.style.left =
        `${(x / boardRect.width) * 100}%`;

      note.style.top =
        `${(y / boardRect.height) * 100}%`;
    }

    async function onMouseUp(ev) {

      document.removeEventListener(
        "mousemove",
        onMouseMove
      );

      document.removeEventListener(
        "mouseup",
        onMouseUp
      );

      setDragging(false);

      let x =
        ev.clientX -
        boardRect.left -
        offsetX;

      let y =
        ev.clientY -
        boardRect.top -
        offsetY;

      x =
        (x / boardRect.width) * 100;

      y =
        (y / boardRect.height) * 100;

      await updatePosition(
        todo.id,
        x,
        y
      );

      refresh();
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