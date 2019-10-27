<h1>
 <img src="sc_logo.png" height='100px'> sharpcoin
</h1>

Sharpcoin is a cryptocurrency implementation that follows the same principals as Bitcoin, it is inspired by [Naivecoin](https://github.com/conradoqg/naivecoin) and its main goal is not to be a serious contender among all the other coins. This coin should be considered a toycoin. Originally sharpcoin was developed for educational purposes, however it is a fully operational coin.

This repository contains code in c# and is called the core. It manages the entire backend of the coin and offers a programmable interface through a TCP server. For easy access to the API see the [sharpcoin-lib](https://github.com/aal89/sharpcoin-lib) package.

## Features

* The blockchain.
  * Indexes.
  * Transactions.
  * Blocks.
* Cryptographic functions.
  * Keypair generation (ECDSA P-256 curve).
  * Message/transaction signing and verification.
* A simple miner.
* p2p logic.
  * Blockchain synch.
  * Transactions/peers propagation.
  * UPnP port mapping to increase connectivity behind NATs.
* Programmable interface.
* Written using dotnetcore v3.0, released for different platforms as a single executable.

Notably you won't find a graphical interface in the core. You can build one yourself implementing the API the core exposes.

### Blockchain
Sharpcoin, like Bitcoin, is a distributed application which tries to maintain state across all of it's nodes everywhere in the world. This can be tedious task, especially when the network is decentralised (p2p) and you want to prevent duplicate data (double spending).
Effectively a blockchain is an immutable public verifiable linked list. Often with regard to the users anonymity. A blockchain contains blocks that are linked together through their index numbers and pointers, which creates the (conceptual) chain. Blocks can hold any form of data. In a cryptocurrency implementation this data are transactions.

#### Blocks
Blocks -- or new elements on the linked list -- are added slowly. About one in every 10 minutes. A block is considered valid when the proof-of-work can be verified (among other things, see `IsValidBlock()` in `Blockchain.cs`). A block can contain one or more transactions, the maximum block size in sharpcoin is 2MB. Once a block is mined the reward contains 50 coins, reward degradation does not exist in sharpcoin.

#### Transactions
A normal (not a reward) transaction is never self-contained in such a cryptocurrency. Sharpcoin implements a transaction based blockchain. This means that each transaction will require you to use another transaction as it's input in order to generate new (unspent) outputs. Essentially there are only unspent outputs (utxo's), they are the 'coin'. These utxo's are indexed in the blockchain and quickly queryable. No fee transactions exists in sharpcoin, miners will always be rewarded 50 coins.

### Cryptographic functions
The core uses the NIST P-256 elliptic curve as part of the ECDSA algorithm. It is used to generate signatures and to verify them. You will also find a small abstraction over this algorithm called `SharpKeyPair` and it can be considered the wallet (although this concept/terminology does not exist in the core). With a `SharpKeyPair` you can mine blocks and create transactions.

### Miner
Included is a very simple, not optimised, mining function. It can be used to make blocks acceptable. A block is accepted when it's hash evaluates to number smaller than the current target difficulty of the blockchain. See the `Config.cs` file for functions regarding targethash calculation.

### p2p
Sharpcoin manages a decentralised network. Running the core will connect you with up to ten different peers randomly (if added any). Every 15 minutes you and each other node will broadcast a list of known sharpcoin peers. When you are new to the network it can take a while before your ip address is known throughout the network.
To-be mined transactions and mined blocks are also broadcasted to each connected peer. When the network is properly connected a truth (consensus) appears and the same blockchain exists at all peers everywhere. This requires continually synching up the chain and peers with each other. Blockchain size (height) checks are also done every 15 minutes.

To increase connectivity to the network enable UPnP features of your router.

### API
You can control or read values from the core through the programmable interface. Using a TCP connection you can request any block or transaction. It is also possible to generate new `SharpKeyPair`s or to start/stop the miner. Inspect this [npm package](https://github.com/aal89/sharpcoin-lib) to learn more about connecting to the core as a client, rather than a peer.

## Build

You can build the product yourself or download a binary, see releases. Dotnet core v3 is required, download the latest [Visual Studio](https://visualstudio.microsoft.com/thank-you-downloading-visual-studio/?sku=Community&rel=16) to get a complete package. Clone the project and in the root execute the following command.

```sh
dotnet publish -c Release -r [rid] --self-contained true /p:PublishSingleFile=true
```

Following the `-r` parameter is a platform indication. As called by Microsoft a Runtime IDentifier (rid). To build for a different platform/arch use a different value. See [this page](https://docs.microsoft.com/en-us/dotnet/core/rid-catalog#windows-rids) for more information. Note that building for linux environments work, but keypair operations fail for such systems. However, verification of all stuff seems to work so you can use it as a full node, just no mining features available. It is a known issue.

The output should contain one warning (yellow text) to indicate a dependency its target SDK is of a different version, this is ok. Afterwards in the directory `Core/bin/Release/netcoreapp3.0/[rid]/publish` you will find the built binaries.

## Usage

Either build the product yourself or head over to releases and pick a prebuilt binary for your platform/arch. The build process or the downloaded zip contains two files; `core.exe`, `core.pdb`. Of course the extensions of the executable differ from platform to platform, but we stick with the Windows platform for now.

Run the executable `core.exe` if all goes well you should see output similar to the following.

```sh
10/23/2019 20:31:09 [Core] sharpcoin (core) v0.1 -- by aal89
10/23/2019 20:31:09 [Core] Attempting to bind tcp servers to address: 169.254.8.200.
10/23/2019 20:31:09 [Core] Initializing blockchain.
10/23/2019 20:31:09 [Blockchain] Loading tx index.
10/23/2019 20:31:09 [Blockchain] Loading utxo index.
10/23/2019 20:31:09 [Blockchain] Validating blockchain.
10/23/2019 20:31:09 [Blockchain] Valid!
10/23/2019 20:31:09 [Core] Setting up event listeners...Done.
10/23/2019 20:31:09 [Core] Setting up client management...Done.
10/23/2019 20:31:09 [Core] Setting up mine operator...Done.
10/23/2019 20:31:09 [Core] Setting up peer manager.
10/23/2019 20:31:09 [PeerManager] Creating UPnP port mapping.
10/23/2019 20:31:10 [PeerManager] Successfully created UPnP port mapping.
10/23/2019 20:31:10 [PeerManager] Setting up server and accepting connections...
```

A folder called `blockchain` and a file called `peers.txt` will be created right next to the other two files. You'll end up with three files and one folder total.

As you can see in the example output is that UPnP port mapping was created succesfully. It will attempt to redirect traffic coming in at port `18910` of your public ip to the machine indicated by the second log line. If you'd like to bind the core to a different ip address you can do so by supplying it as an argument when launching.

```sh
core.exe 192.168.178.100
```

The last log line (`Setting up server and accepting connections...`) is an indication that the core is done booting and is ready for use. Connect and control it via the port `28910`. This port should not be exposed to the outside, so watchout with hosts running in the DMZ of your router. If you want to connect to the core using Javascript/Typescript use this [library](https://github.com/aal89/sharpcoin-lib).

### Connecting to the network

So far you have a sharpcoin node running in isolation to connect to the current operational network close `core.exe` and open the `peers.txt` file and add this ip address: `::ffff:37.97.206.23`. Don't forgot the end the file with a newline. Close and save the file. Now start `core.exe` again. You should see the output from above, but also some synchronisation happening. The most updated blockchain is being downloaded and verified.

To start mining or use any of the other features, read the documentation in the [sharpcoin-lib](https://github.com/aal89/sharpcoin-lib).
