# Motion Highlighter

Motion Highlighter is a Windows Forms application using **OpenCvSharp** to detect, highlight, and export motion from video files (MP4, MOV, MKV, AVI) and live video streams (MPEG-DASH, HLS, Fragmented MP4, M4V, MJPEG, RTSP).

## Features

- **Motion Detection**: Running average background subtraction with adjustable sensitivity.
- **Streaming Video Support**:
  - Live stream motion highlighting for MPEG-DASH (`.mpd`), HLS (`.m3u8`), Fragmented MP4 (`.m4s`), MPEG-4 Video (`.m4v`), MJPEG, RTSP, and Web Live Feeds.
  - **Custom Camera & Credentials Config (`streams.config.json`)**: Pre-configure custom cameras, local RTSP/MJPEG feeds, and authentication credentials (basic auth usernames/passwords, bearer tokens, cookies) in a single JSON file. Click `⚙ Config JSON` in the stream dialog to open and edit it in your default editor.
  - **Camera Portals & Stream Presets**: Choose directly from pre-configured camera portals (Google Home, Google Nest, Ring Protect, Arlo, Wyze, Eufy, UniFi Protect) or public test streams (Big Buck Bunny DASH / HLS).
  - **Persistent Stream History**: Automatically remembers previously connected camera streams and authenticated URLs across sessions for instant 1-click re-selection.
  - **In-App Browser & Auto-Capture**: Built-in browser with address bar navigation, back/forward/refresh controls, quick portal jumping, and automatic active `<video>` and `<canvas>` feed detection for any authenticated website.
  - **URL Authentication**: Supports Bearer Tokens, HTTP Basic Auth (username/password), custom request headers (cookies, referer, API keys), and URL query parameters.
  - **Webpage Stream Extraction**: Automatically parses camera portals or web pages to discover embedded video elements, manifests, and JavaScript player configurations.
  - **Chunk & Fragment Grabber**: Automatically resolves and downloads fMP4 initialization segments (`init.m4s` / `init.mp4`) and consecutive media segments (`.m4s` / `.m4v` / `.ts`), demuxing and decoding them live into the motion detection pipeline.
  - **Stream-Safe Controls**: Disables backward/forward scrubbing to maintain real-time live sync; provides Play, Pause, Stop, and instant screenshot grabbing.
  - **Screenshot Grabbing**: Capture screenshots of the current frame (with motion bounding boxes and overlay masks) with one click (`📸 Screenshot`) or shortcut (`S`), saving directly to the Pictures folder and copying to the clipboard.
- **Video Zoom & Detail Inspection**:
  - **Anchored Zoom (1.0x – 10.0x / 100% – 1000%)**: Zoom into any region of the video or live stream to inspect motion-highlighted objects and bounding boxes in fine detail.
  - **Mouse Wheel Zoom**: Hold **Ctrl + Mouse Wheel** over the video (or roll the wheel during live streams) to zoom smoothly anchored directly at the mouse cursor.
  - **Interactive Panning (Click & Drag)**: When zoomed in, click and drag with the left or middle mouse button to pan smoothly across the frame with 1:1 pixel accuracy.
  - **Mini-Map HUD Navigator & Badge**: A translucent mini-map thumbnail in the bottom-right corner displays your current viewport relative to the full frame, accompanied by an on-screen zoom badge.
  - **Adaptive Interpolation**: Automatically switches from smooth High-Quality Bicubic scaling at low zoom ($\le 3\times$) to crisp Nearest-Neighbor rendering at high zoom ($> 3\times$) for pixel-accurate inspection without blur.
  - **Toolbar Controls & Shortcuts**: Dedicated `🔍 -`, `100%`, `🔍 +`, and `Fit` buttons on the control bar, plus keyboard shortcuts:
    - **Ctrl + Plus / Minus**: Zoom in or out.
    - **Ctrl + 0 / Escape / Double-Click**: Reset zoom to fit.
  - **Zoomed Detail Screenshot**: When zoomed in, clicking `📸 Screenshot` or pressing `S` captures and copies the exact zoomed-in detail view at full resolution.
- **Visual Overlays**:
  - Semi-transparent red motion mask with adjustable opacity.
  - Option to show or hide motion trails in the red overlay.
  - Green bounding boxes around active moving objects with ghost/trail rejection.
- **Trail Rejection for Bounding Boxes**:
  - Distinguishes active motion from lingering background-adaptation trails.
  - Green bounding boxes track the active moving subject and do not pick up on historical trails.
- **Interactive Video Scrubbing & Scrolling**:
  - Scrub smoothly with the timeline slider.
  - Roll the mouse wheel over the video player or timeline to scrub frame by frame:
    - **Mouse Wheel**: Step 1 frame forward or backward (or zoom in live streams).
    - **Ctrl + Mouse Wheel**: Zoom in and out anchored at cursor.
    - **Shift + Mouse Wheel**: Step 1 second forward or backward.
  - Frame stepping buttons: `⏮ -1s`, `◀ -1f`, `▶ Play/Pause`, `+1f ▶`, `+1s ⏭`, `⏹ Stop`.
  - Keyboard shortcuts:
    - **Space**: Toggle Play / Pause.
    - **Left / Right Arrow**: Step backward / forward 1 frame (hold **Shift** to step 1 second).
    - **Home / End**: Jump to video start or end.
- **Instant Parameter Feedback**:
  - Adjusting Threshold, Min Area, Blur Kernel, or Overlay Opacity immediately re-renders the current frame when paused without reloading.
- **Exporting**:
  - Background asynchronous export to MP4 with progress reporting and cancellation support.

## Detection Settings

- **Threshold (1–255)**: Difference in pixel intensity required to trigger motion detection.
- **Min Area (pixels²)**: Minimum bounding contour area required to register a motion region (minimum 50 px²). Filters out camera noise, dust, insects, and compression artifacts to keep CPU usage low.
- **Blur Kernel (odd numbers)**: Size of the Gaussian blur kernel used to filter noise.
- **Learn Rate (0–100%)**: Rate at which the background model adapts to changes over time.
- **Reset Background Model**: Reset the background reference model at any time to recalibrate.
