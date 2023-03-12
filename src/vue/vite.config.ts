import { defineConfig } from 'vite'
import vue from '@vitejs/plugin-vue'
import path from 'path'
import { execSync } from 'child_process'
import { readFileSync } from 'fs'

if (process.env.VERCEL) {
  console.log("fetching full Git commit history")
  execSync('git remote add --fetch --tags origin https://github.com/joelverhagen/FactorioTools.git')
  execSync('git fetch --depth 1000000')
  execSync('git fetch --all --tags')
}

const buildDate = new Date().toISOString().slice(0, 10);

let branch = execSync('git rev-parse --abbrev-ref HEAD').toString().trim()
if (process.env.VERCEL_GIT_COMMIT_REF) {
  branch = process.env.VERCEL_GIT_COMMIT_REF.trim()
}

const commit = execSync('git rev-parse --short HEAD').toString().trim()
const shortVersion = execSync('git describe --abbrev=0').toString().trim()
let version = execSync('git describe --long --dirty').toString().trim()
if (version == `${shortVersion}-0-g${commit}` && branch == 'main') {
  version = shortVersion
} else {
  version += "-" + branch
}

const sampleBlueprints = Array.from(new Set(readFileSync("../../test/FactorioTools.Test/OilField/blueprints.txt", { encoding: 'utf8' })
  .split(/\r?\n/)
  .map(l => l.trim())
  .filter(l => l.length > 0 && !l.startsWith("#"))))

const basePath = (process.env.BASE_PATH || '').trim().replace(/\/+$/g, '') + '/'
const define = {
  __BASE_PATH__: JSON.stringify(basePath),
  __BUILD_DATE__: JSON.stringify(buildDate),
  __GIT_BRANCH__: JSON.stringify(branch),
  __GIT_COMMIT__: JSON.stringify(commit),
  __GIT_VERSION__: JSON.stringify(version),
  __SAMPLE_BLUEPRINTS__: JSON.stringify(sampleBlueprints),
}

console.log('build-time config:')
for (const [key, value] of Object.entries(define)) {
  if (key == "__SAMPLE_BLUEPRINTS__") {
    console.log(` - ${key}: (${sampleBlueprints.length} blueprints)`);
  } else {
    console.log(` - ${key}: ${value}`);
  }
}

// https://vitejs.dev/config/
export default defineConfig({
  resolve: {
    alias: {
      '~bootstrap': path.resolve(__dirname, 'node_modules/bootstrap'),
    }
  },
  plugins: [vue()],
  define: define,
  base: basePath
})
