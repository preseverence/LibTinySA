# LibTinySA
TinySA radio scanner interface lib.

The primary goal was to have a C# API lib for COM/USB connection.

The real readme is on the go, but currently you might use `TinySAControl` class for connection. Anyway you would need a platform specific COM emulation connection interface.
`LibTinySA.Windows` contains helper classes for Windows environment - `SerialPort`-based interface and ImageHelper to transform device's screenshots to `System.Drawing.Bitmap`.