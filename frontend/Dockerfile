# Runtime image only — the CI pipeline runs `npm run build` before this,
# so .output/ already exists on disk when this Dockerfile builds.
FROM node:22-alpine
WORKDIR /app

# The server only needs `node`. The package managers bundled with the base image bring
# HIGH vulnerabilities (Trivy) and are never used at runtime, so they are removed.
RUN rm -rf /usr/local/lib/node_modules /opt/yarn-* \
  && rm -f /usr/local/bin/npm /usr/local/bin/npx /usr/local/bin/corepack \
     /usr/local/bin/yarn /usr/local/bin/yarnpkg

ENV PORT=8080
ENV HOST=0.0.0.0
EXPOSE 8080

COPY .output ./.output

CMD ["node", ".output/server/index.mjs"]
