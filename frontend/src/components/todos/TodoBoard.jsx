import { useEffect, useState } from "react";

import {
  getTodos,
  createTodo,
  deleteTodo,
  updateTodo,
  updatePosition
} from "../../api/todoApi";

import TodoNote from "./TodoNote";
import TodoModal from "./TodoModal";

export default function TodoBoard() {
  const [todos, setTodos] = useState([]);
  const [selectedDate, setSelectedDate] = useState(
    new Date().toISOString().split("T")[0]
  );
  const [showModal, setShowModal] = useState(false);
  const [editingTodo, setEditingTodo] = useState(null);

  useEffect(() => {
    loadTodos();
  }, [selectedDate]);

async function loadTodos() {
    console.log('Loading todos for date:', selectedDate);  // ← отладка
    
    try {
        const data = await getTodos(selectedDate);
        console.log('Loaded todos:', data);  // ← отладка
        setTodos(data);
    } catch (error) {
        console.error('Load error:', error);
    }
}

  async function handleToggleComplete(todo) {
    await updateTodo(todo.id, {
      ...todo,
      isCompleted: !todo.isCompleted
    });
    await loadTodos();  // ✅ обновляем ПОСЛЕ изменения
  }

async function handleSave(todoData) {
    console.log('Saving todo:', todoData);  // ← отладка
    
    try {
        if (editingTodo) {
            await updateTodo(editingTodo.id, { ...editingTodo, ...todoData });
        } else {
            await createTodo({ ...todoData, date: selectedDate });
        }
        
        setShowModal(false);
        setEditingTodo(null);
        await loadTodos();  // ← дождаться загрузки
    } catch (error) {
        console.error('Save error:', error);
        alert('Failed to save task');
    }
}

  return (
    <div className="board-warpper">
      <div className="board-header">
        <button
          onClick={() => {
            const d = new Date(selectedDate);
            d.setDate(d.getDate() - 1);
            setSelectedDate(d.toISOString().split("T")[0]);
          }}
        >
          ←
        </button>

        <div>
          <h1>
            {new Date(selectedDate).toLocaleDateString("en-US", {
              weekday: "long"
            })}
          </h1>
          <p>{selectedDate}</p>
        </div>

        <button
          onClick={() => {
            const d = new Date(selectedDate);
            d.setDate(d.getDate() + 1);
            setSelectedDate(d.toISOString().split("T")[0]);
          }}
        >
          →
        </button>
      </div>

      <button
        className="add-note-btn"
        onClick={async () => {
          setEditingTodo(null);
          setShowModal(true);
          await loadTodos();
        }}
      >
        ✨ Add task
      </button>

      <div className="board">
        {todos.map((todo, index) => (
          <TodoNote
            key={todo.id}
            todo={todo}
            index={index}
            onDelete={async () => {
              await deleteTodo(todo.id);
              await loadTodos();
            }}
            onEdit={async () => {
              setEditingTodo(todo);
              setShowModal(true);
            }} 
            onToggleComplete={() => handleToggleComplete(todo)}
          />  
        ))}  
      </div> 

      {showModal && (
        <TodoModal
          todo={editingTodo}
          onClose={async () => {setShowModal(false);
          
          await loadTodos();}}
          onSave={handleSave
          }
          
        />
      )}
    </div>  
   
  );  
}  