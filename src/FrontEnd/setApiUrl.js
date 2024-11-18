/* eslint-disable @typescript-eslint/no-var-requires,no-console */ // Node, at the time of writing, doesn't support `import`.
const fs = require("fs");
const path = require("path");

const [, , clientName] = process.argv;

if (!fs.existsSync("./config/settings.js")) {
    fs.copyFileSync(path.resolve(__dirname, "./config/settings-base.js"), "./config/settings.js");
}

function setApiUrl(clientName){
    const config = fs.readFileSync("./config/settings.js", "utf-8");
    const newConfig = config.replace("commerce", clientName)
    fs.writeFileSync("./config/settings.js", newConfig, "utf-8");
}

setApiUrl(clientName);
