#!/usr/bin/env python3
"""极简 xxl-job 执行器：只接 /run 立即回 200，记录每次触发到达的毫秒时刻。
   同时每 20s 向调度中心注册一次，维持 xxl_job_registry 存活。"""
import asyncio, time, json, threading, urllib.request, sys, os

PORT = 9977
ADMIN = "http://localhost:8088/xxl-job-admin/api/registry"
GROUP = "xxl-job-executor-sample"
ADDR = "http://host.docker.internal:9977/"
OUT = sys.argv[1] if len(sys.argv) > 1 else "/tmp/xxlbench/run.tsv"

records = []
resp_body = b'{"code":200,"msg":null}'
resp = (b"HTTP/1.1 200 OK\r\nContent-Type: application/json\r\nContent-Length: "
        + str(len(resp_body)).encode() + b"\r\nConnection: keep-alive\r\n\r\n" + resp_body)


async def handle(reader, writer):
    try:
        head = await reader.readuntil(b"\r\n\r\n")
        cl = 0
        for line in head.split(b"\r\n"):
            low = line.lower()
            if low.startswith(b"content-length:"):
                cl = int(line.split(b":", 1)[1].strip())
        if cl:
            await reader.readexactly(cl)
        records.append(time.time_ns() // 1_000_000)
        writer.write(resp)
        await writer.drain()
    except Exception:
        pass
    finally:
        try:
            writer.close()
        except Exception:
            pass


def flusher():
    f = open(OUT, "a", buffering=1 << 20)
    while True:
        time.sleep(1)
        if records:
            batch, records[:] = records[:], []
            f.write("".join(f"{t}\n" for t in batch))
            f.flush()


def registrar():
    body = json.dumps({"registryGroup": "EXECUTOR", "registryKey": GROUP,
                       "registryValue": ADDR}).encode()
    while True:
        try:
            req = urllib.request.Request(ADMIN, data=body,
                                         headers={"Content-Type": "application/json",
                                                  "XXL-JOB-ACCESS-TOKEN": "default_token"})
            r = urllib.request.urlopen(req, timeout=3)
            txt = r.read().decode()
            if '"code":200' not in txt:
                print("reg rejected:", txt, flush=True)
        except Exception as e:
            print("reg fail:", e, flush=True)
        time.sleep(20)


async def main():
    threading.Thread(target=flusher, daemon=True).start()
    threading.Thread(target=registrar, daemon=True).start()
    server = await asyncio.start_server(handle, "0.0.0.0", PORT, backlog=4096)
    print(f"fake executor on :{PORT} -> {OUT}", flush=True)
    async with server:
        await server.serve_forever()


if __name__ == "__main__":
    try:
        asyncio.run(main())
    except KeyboardInterrupt:
        pass
