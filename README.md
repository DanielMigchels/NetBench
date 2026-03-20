# NetBench

Free and open-source .NET network benchmarking tool in the spirit of [iPerf](https://iperf.fr/). Measure TCP throughput between any two machines on your network.

<img style="width: 600px;" src="NetBench.Docs/demo_client.png">
<img style="width: 600px;" src="NetBench.Docs/demo_server.png">

## Installation

Download the latest release from the [Releases](https://github.com/DanielMigchels/NetBench/releases) page.

## Usage

```
netbench [options]
```

### Options

| Flag | Description |
|------|-------------|
| `-s <host:port>` | Start a TCP server listening on the given address and port. Can be specified multiple times. |
| `-c <host:port>` | Start a TCP client connecting to the given server. Can be specified multiple times. |
| `-l <mbps>` | Limit client send throughput to the specified rate in Mbps. |
| `-o <file>` | Write a network report to the specified file. |

### Quick Start

**1. Start a server** on the host machine, binding to all interfaces on port 5000:

```
netbench -s 0.0.0.0:5000
```

**2. Start a client** on another machine, pointing at the server's IP:

```
netbench -c 192.168.1.100:5000
```

The client will send data as fast as possible and both sides will report measured throughput in Mbps. Press `Ctrl+C` to stop.

### Examples

Run a server and client on the same machine:

```
netbench -s 127.0.0.1:5000 -c 127.0.0.1:5000
```

Limit client throughput to 10 Mbps:

```
netbench -c 192.168.1.100:5000 -l 10
```

Write results to a file:

```
netbench -c 192.168.1.100:5000 -o report.txt
```

Multiple servers and clients in a single command:

```
netbench -s 0.0.0.0:5000 -s 0.0.0.0:5001 -c 192.168.1.100:5000 -c 192.168.1.100:5001
```
