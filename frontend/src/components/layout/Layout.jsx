import TodoBoard from "../todos/TodoBoard";
import ProfileModal from "../profile/ProfileModal";
import { useEffect } from "react";
import { getProfile } from "../../api/profileApi";

import cake from "../../../images/cake.png";
import tea from "../../../images/tea.png";
import cookie from "../../../images/cookie.png";
import drink from "../../../images/drink.png";

import { useState } from "react";

export default function Layout() {

  const [showProfile, setShowProfile] =
    useState(false);

  const [profile, setProfile] =
    useState(null);

useEffect(() => {
  loadProfile();
}, []);

async function loadProfile() {

  try {

    const data = await getProfile();

    console.log("PROFILE:", data);

    setProfile(data);

  } catch (err) {

    console.error(err);
  }
}
  return (

    <div className="planner-page">

      {/* PROFILE BUTTON */}

      <div
        className="user-pill"
        onClick={() =>
          setShowProfile(!showProfile)
        }
      >
        👤 {profile?.name || "loading"}
      </div>

      {/* PROFILE MODAL */}

      {showProfile && (

        <div className="profile-overlay">

          <ProfileModal
            profile={profile}
            onClose={() =>
              setShowProfile(false)
            }
          />

        </div>
      )}

      {/* DECOR */}

      <img
        src={cake}
        className="decor decor-1"
      />

      <img
        src={tea}
        className="decor decor-2"
      />

      <img
        src={drink}
        className="decor decor-3"
      />

      <img
        src={cookie}
        className="decor decor-4"
      />

      {/* TASKS */}

      <section
        className="
          planner-section
          tasks-section
        "
      >

        <h2 className="section-title">
          ✨ Tasks
        </h2>

        <TodoBoard />

      </section>

      {/* HABITS */}

      <section
        className="
          planner-section
          habits-section
        "
      >

        <h2 className="section-title">
          🌱 Habits
        </h2>

        <p>Coming soon...</p>

      </section>

      {/* MOOD */}

      <section
        className="
          planner-section
          mood-section
        "
      >

        <h2 className="section-title">
          😊 Mood
        </h2>

        <div className="mood-row">
          😀 😌 😴 😭 😡
        </div>

      </section>

    </div>
  );
}