// Preview server for src/CommandLine/_public.
// Watch only page assets — media/PDF copies during site generation lock files
// and crash Chokidar (EBUSY) if they are watched.
const path = require("path");

const publicPath = path.resolve(__dirname, "..", "src", "CommandLine", "_public");

module.exports = {
  server: publicPath,
  files: [
    path.join(publicPath, "**", "*.html"),
    path.join(publicPath, "**", "*.css"),
    path.join(publicPath, "**", "*.js"),
  ],
  watchOptions: {
    ignoreInitial: true,
    awaitWriteFinish: {
      stabilityThreshold: 400,
      pollInterval: 100,
    },
  },
  reloadDebounce: 500,
  open: true,
  notify: false,
};
