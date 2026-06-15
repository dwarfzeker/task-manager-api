// hooks/useProfile.js
import { getProfile } from "../api/profileApi";
import { useEffect, useState } from "react";

export default function useProfile(dependencies = []) {
  const [profile, setProfile] = useState(null);
  const [loading, setLoading] = useState(true);

  const refreshProfile = async () => {
    try {
      const data = await getProfile();
      setProfile(data);
    } catch (err) {
      console.error("Failed to load profile", err);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    refreshProfile();
  }, dependencies);

  return { profile, loading, refreshProfile };
}