import { useEffect, useRef, useState } from 'react';
import './custom.css';

export default function App() {
  const socketRef = useRef<WebSocket | null>();
  const [log, setLog] = useState("");

  const createWebSocket = (fileName: string) => {
    const webSocket = new WebSocket(`ws://localhost:5123/ws/TrailLog?fileName=${fileName}`)
    socketRef.current = webSocket

    // サーバーからの通知に対するコールバックの設定
    webSocket.addEventListener("message", event => {
      const response = JSON.parse(event.data);
      setLog(log => log + response.Log);
    })
  }

  const closeSocket = () => {
    socketRef.current?.close()
    socketRef.current = null;
  }

  const sendLogCommand = ((command: number) => {
    socketRef.current?.send(JSON.stringify({
      LogCommand: command
    }))
  })
  
  useEffect(() => {
    // 30秒に一度Ping
    const intervalId = setInterval(() => {
      sendLogCommand(1)
    }, 30000)

    // ソケット通信の開始
    createWebSocket("log.txt")

    // 画面遷移時の破棄処理
    return () => {
      clearInterval(intervalId);
      closeSocket();
    }
  }, [])
  return (
    <pre>{log}</pre>
  );
}
