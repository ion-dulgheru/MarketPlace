import { defineConfig } from "vite";
import { tanstackStart } from "@tanstack/react-start/plugin/vite";
import viteReact from "@vitejs/plugin-react";
import tailwindcss from "@tailwindcss/vite";
import tsconfigPaths from "vite-tsconfig-paths";
import { nitro } from "nitro/vite";

// Plain Vite + TanStack Start config — replaces @lovable.dev/vite-tanstack-config.
// That wrapper's prerender pipeline only runs for its own pinned Cloudflare presets
// (cloudflare-module, lovable-fetch-bundle), so switching to the node-server preset
// (needed to self-host on a plain Node server) silently skipped prerendering and
// left the app serving an unrendered dev-style index.html. Composing the plugins
// directly here gives full control over prerender/SPA behavior for any preset.
//
// nitro is build-only: including it during `vite dev` replaces TanStack Start's
// SSR dev environment with nitro's own (buggy, in this nitro 3.x beta) dev-time
// routing, which never wires the SSR handler up — every request falls back to a
// raw, unrendered index.html. Only add nitro for the actual build.
export default defineConfig(({ command }) => ({
  resolve: {
    dedupe: ["react", "react-dom"],
  },
  plugins: [
    tanstackStart({
      server: { entry: "server" },
    }),
    viteReact(),
    tailwindcss(),
    tsconfigPaths(),
    ...(command === "build" ? [nitro({ preset: "node-server" })] : []),
  ],
}));
