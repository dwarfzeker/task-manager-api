export default function Sidebar({ page, setPage }) {

  return (
    <aside className="sidebar">

      <h2>✨ Planner</h2>

      <button
        className={page === "todos" ? "active" : ""}
        onClick={() => setPage("todos")}
      >
        📝 Todos
      </button>

      <button
        className={page === "habits" ? "active" : ""}
        onClick={() => setPage("habits")}
      >
        🌱 Habits
      </button>

      <button
        className={page === "diary" ? "active" : ""}
        onClick={() => setPage("diary")}
      >
        📖 Diary
      </button>

    </aside>
  );
}