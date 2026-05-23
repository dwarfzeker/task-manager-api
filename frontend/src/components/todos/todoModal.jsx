import { useState } from "react";

export default function TodoModal({
  onSave,
  onClose
}) {

  const [title, setTitle] =
    useState("");

  const [description, setDescription] =
    useState("");

  return (

    <div className="modal-overlay">

      <div className="todo-modal">

        <button
          className="close-modal"
          onClick={onClose}
        >
          ✕
        </button>

        <h2>✨ New task</h2>

        <input
          placeholder="Title..."
          value={title}
          onChange={(e) =>
            setTitle(e.target.value)
          }
        />

        <textarea
          placeholder="Description..."
          value={description}
          onChange={(e) =>
            setDescription(e.target.value)
          }
        />

        <button
          className="save-btn"
          onClick={() =>
            onSave({
              title,
              description
            })
          }
        >
          Add task
        </button>

      </div>

    </div>
  );
}