# Network Events

THESE ARE BYTES

## Client -> Server

### `0x01` Initiate Handshake/Join game

No params.

### `0x02` Send clients location

| X Pos | Y Pos | Z Pos | X Rot | Y Rot | Z Rot | W Rot | 
| --- | --- | --- | --- | --- | --- | --- |
| float | float | float | float | float | float | float |


## Server -> Client

### `0x01` Handshake response

| Player ID | Spawn X | Spawn Y | Spawn Z |
| --------- | --- | --- | --- |
| uint8 | float | float | float |
| 0-255 identifies player | x | y | z |

### `0x02` Update client of other player locations

| Length | Player ID | Player X Pos | Player Y Pos | Player Z Pos | ... |
| --- | --- | --- | --- | --- | --- |
| uint8 | uint8 | float | float | float |  |
| | | | | | Length - 1 additional sets of player data |


### `0x03` Round start warning

### `0x04` Round start notification

### `0x05` Round end warning

### `0x06` Round end notification



