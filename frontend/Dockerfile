# Runtime image only — the CI pipeline runs `npm run build` before this,
# so .output/ already exists on disk when this Dockerfile builds.
FROM node:22-alpine
WORKDIR /app

ENV PORT=8080
ENV HOST=0.0.0.0
EXPOSE 8080

COPY .output ./.output

CMD ["node", ".output/server/index.mjs"]
