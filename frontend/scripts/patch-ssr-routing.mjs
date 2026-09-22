// Workaround for a nitro 3.x (beta) + TanStack Start bug: when Vite's SSR
// environment is built as a separate lazy "service" (services.ssr), the
// generated node-server route table never wires it up — every request falls
// through to nitro's generic renderer-template fallback (raw, unbuilt
// index.html), instead of the actual SSR-rendered page. This patches the
// built server entry to route "/**" directly to the SSR service.
import { readFileSync, writeFileSync } from "node:fs";

const entryPath = ".output/server/index.mjs";
const source = readFileSync(entryPath, "utf8");

const pattern =
  /var (\w+) = defineLazyEventHandler\(\(\) => import\("\.\/_chunks\/renderer-template\.mjs"\)\);/;
const match = source.match(pattern);

if (!match) {
  console.warn(
    "[patch-ssr-routing] Pattern not found — nitro's output may have changed shape. " +
      "Skipping patch; verify SSR rendering still works.",
  );
  process.exit(0);
}

const varName = match[1];
const patched = source.replace(
  pattern,
  `var ${varName} = defineHandler((event) => services.ssr.fetch(event.req));`,
);

writeFileSync(entryPath, patched);
console.log("[patch-ssr-routing] Wired the SSR service into the route table.");
