import { logout } from "../../api/authApi";

export default function ProfileModal({ onClose, profile }) {

  if (!profile) {
    return (
      <div className="profile-popup">
        <button className="close-profile" onClick={onClose}>✕</button>
        <div>Loading profile...</div>
      </div>
    );
  }

  const handleLogout = async () => {
    try {
      await logout();
      onClose(); 
      window.location.reload(); 
    } catch (err) {
      console.error("Logout failed:", err);
    }
  };

  return (
    <div className="profile-popup">
      {/* КРЕСТИК ДЛЯ ЗАКРЫТИЯ */}
      <button className="close-profile" onClick={onClose}>
        ✕
      </button>

      <h2>✨ Your stats</h2>

      <div className="stat-row">
        <span>✅ Completed </span>
        <b>{profile.completedTasks}</b>
      </div>

      <div className="stat-row">
        <span>📋 Total </span>
        <b>{profile.totalTasksCount || 0}</b>
      </div>

      <div className="stat-row">
        <span>🔥 Streak </span>
        <b>{profile.currentStreak || 0}</b>
      </div>

      <div className="stat-row">
        <span>🏆 Best streak </span>
        <b>{profile.longestStreak || 0}</b>
      </div>

      <div className="stat-row">
        <span>📈 Completion </span>
        <b>{Math.round(profile.completionRate || 0)}%</b>
      </div>

      <div className="stat-row">
        <span>🕒 Last activity </span>
        <b>
          {profile.lastActivity
            ? new Date(profile.lastActivity).toLocaleDateString()
            : "No activity"}
        </b>
      </div>

      <button className="logout-btn" onClick={handleLogout}>
        Logout
      </button>
    </div>
  );
}