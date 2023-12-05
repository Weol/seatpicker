type Config = {
  ApiHost: string
  ApiBaseUrl: string
  DiscordAvatarBaseUrl: string
  DiscordGuildIconBaseUrl: string
}

const host = window.location.host
let baseUrl = "https://" + host + "/api"

if (host.includes("localhost")) {
  baseUrl = "http://" + host
}

const config: Config = {
  ApiHost: host,
  ApiBaseUrl: baseUrl,
  DiscordAvatarBaseUrl: "https://cdn.discordapp.com/avatars/",
  DiscordGuildIconBaseUrl: "https://cdn.discordapp.com/icons/",
}

export default config
