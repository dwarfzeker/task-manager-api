import { useEffect, useState } from "react";
import {getMe} from "./api/authApi";

import AuthPage from "./components/auth/AuthPage";
import Layout from "./components/layout/Layout";
import "../styles.css"
export default function App() {
  const [user, setUser] = useState(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    async function checkAuth() {
      try {
        const me = await getMe();
        setUser(me);
      } finally {
        setLoading(false);
      }
    }

    checkAuth();
  }, []);

  if (loading) {
    return <div>Loading...</div>;
  }

  if (!user) {
    return <AuthPage onAuth={setUser} />;
  }

  return <Layout user={user} />;
}