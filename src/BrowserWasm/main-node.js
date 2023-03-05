import { createRequire } from 'module';
import { dirname } from 'path';
import { fileURLToPath } from 'url';
import createDotnetRuntime from './dotnet.js'

const { getAssemblyExports, getConfig } = await createDotnetRuntime(() => ({
    imports: {
        require: createRequire(import.meta.url)
    },
    scriptDirectory: dirname(fileURLToPath(import.meta.url)) + '/',
    disableDotnet6Compatibility: true,
    configSrc: "./mono-config.json",
}));

const config = getConfig();
const exports = await getAssemblyExports(config.mainAssemblyName);
console.log(exports.MyClass.Greeting())
