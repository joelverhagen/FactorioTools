import { defineConfig } from 'vite'
import vue from '@vitejs/plugin-vue'
import path from 'path'
import { execSync } from 'child_process'

const buildDate = new Date().toISOString().slice(0, 10);
const commit = execSync('git rev-parse --short HEAD').toString().trim()
const shortVersion = execSync('git describe --abbrev=0').toString()
let version = execSync('git describe --long --dirty').toString()
console.log(buildDate, commit, shortVersion, version)
if (version == `${shortVersion}-0-g${commit}`) {
  version = shortVersion
}

// https://vitejs.dev/config/
export default defineConfig({
  resolve: {
    alias: {
      '~bootstrap': path.resolve(__dirname, 'node_modules/bootstrap'),
    }
  },
  plugins: [vue()],
  define: {
    __BUILD_DATE__: JSON.stringify(buildDate),
    __GIT_COMMIT__: JSON.stringify(commit),
    __GIT_VERSION__: JSON.stringify(version)
  },
  base: "./"
})
