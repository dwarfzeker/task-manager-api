import { useState } from "react";
import { login, register } from "../../api/authApi";

export default function AuthPage({ onAuth }) {
  const [isLogin, setIsLogin] = useState(true);

  const [form, setForm] = useState({
    userName: "",
    email: "",
    login: "",
    password: ""
  });

  async function handleSubmit() {
    try {
      let data;

      if (isLogin) {
        data = await login({
          login: form.login,
          password: form.password
        });
      } else {
        data = await register({
          userName: form.userName,
          email: form.email,
          password: form.password
        });
      }

      onAuth(data);

    } catch (e) {
      alert(e.message);
    }
  }

  return (
    <div className="auth-page">

      <div className="auth-card">

        <h1>
          {isLogin ? "Welcome back ✨" : "Create account ✨"}
        </h1>

        {!isLogin && (
          <>
            <input
              placeholder="Username"
              value={form.userName}
              onChange={(e) =>
                setForm({ ...form, userName: e.target.value })
              }
            />

            <input
              placeholder="Email"
              value={form.email}
              onChange={(e) =>
                setForm({ ...form, email: e.target.value })
              }
            />
          </>
        )}
        
         <input
          placeholder="Login"
          value={form.login}
          onChange={(e) =>
            setForm({ ...form, login: e.target.value })
          }
        />

        <input
          type="password"
          placeholder="Password"
          value={form.password}
          onChange={(e) =>
            setForm({ ...form, password: e.target.value })
          }
        />

        <button onClick={handleSubmit}>
          {isLogin ? "Login" : "Register"}
        </button>

        <p onClick={() => setIsLogin(!isLogin)}>
          {isLogin
            ? "No account? Register"
            : "Already have account?"}
        </p>

      </div>

    </div>
  );
}