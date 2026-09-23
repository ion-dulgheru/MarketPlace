import { useEffect, useState } from "react";
import heroImg from "./assets/hero.png";
import reactLogo from "./assets/react.svg";
import viteLogo from "./assets/vite.svg";
import "./App.css";
import { httpClient } from "./shared/api/httpClient";

function App() {
  const [count, setCount] = useState(0);
  useEffect(() => {
    httpClient("/health").then(console.log).catch(console.error);
  }, []);

  return <></>;
}

export default App;
