import { useEffect, useState } from "react";

import {
  getTodos,
  createTodo,
  deleteTodo,
  updateTodo,
  updatePosition
} from "../../api/todoApi";
import useProfile from "../../hooks/useProfile"
import TodoNote from "./TodoNote";
import TodoModal from "./todoModal";

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
  
  
  const { profile, refreshProfile } = useProfile([selectedDate]);

async function loadTodos() {
    console.log('Loading todos for date:', selectedDate);  
    
    try {
        const data = await getTodos(selectedDate);
        console.log('Loaded todos:', data); 
        setTodos(data);
        
        setTimeout(() => {
            console.log('Current todos state after setTodos:', todos);
        }, 100);
    } catch (error) {
        console.error('Load error:', error);
    }
}

async function handleToggleComplete(todo) {
    const newCompletedStatus = !todo.isCompleted;
    
    setTodos(prevTodos => 
        prevTodos.map(t => 
            t.id === todo.id 
                ? { ...t, isCompleted: newCompletedStatus }
                : t
        )
    );
    
    try {
        const updatedTodo = await updateTodo(todo.id, {
            title: todo.title,
            description: todo.description || "",
            isCompleted: newCompletedStatus
        });
        
        if (updatedTodo.isCompleted !== newCompletedStatus) {
            console.warn('Server returned different status, syncing...');
            await loadTodos(); 
        }
        await refreshProfile();
        
    } catch (error) {
        console.error('Failed to toggle todo:', error);
        setTodos(prevTodos => 
            prevTodos.map(t => 
                t.id === todo.id 
                    ? { ...t, isCompleted: todo.isCompleted }
                    : t
            )
        );
    }
}

async function handleSave(todoData) {
    console.log('Saving todo:', todoData); 
        const payload = {
            title: todoData.title,
            description: todoData.description || '',
            targetDate: selectedDate  
        };
    try {
        if (editingTodo) {
            await updateTodo(editingTodo.id, { ...editingTodo, ...todoData });
        } else {
            await createTodo({ ...todoData, date: selectedDate });
        }
        
        setShowModal(false);
        setEditingTodo(null);
        await loadTodos();  
        await refreshProfile(); 
    } catch (error) {
        console.error('Save error:', error);
        alert('Failed to save task');
    }
}

  async function handleDelete(id) {
    await deleteTodo(id);
    await loadTodos();
    await refreshProfile(); 
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
              await handleDelete(todo.id);
              await loadTodos();
            }}
            onEdit={async () => {
              setEditingTodo(todo);
              setShowModal(true);
            }} 
            onToggleComplete={() => handleToggleComplete(todo)}
            refresh={loadTodos} 
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