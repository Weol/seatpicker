type Config = {
  ApiHost: string
  ApiBaseUrl: string
  Protocol: string
  DiscordAvatarBaseUrl: string
  DiscordGuildIconBaseUrl: string
}

const host = window.location.host
const protocol = host.includes("localhost") ? "http" : "https"
const apiBaseUrl = host.includes("localhost")
  ? `${protocol}://${host}`
  : `${protocol}://${host}/api`

const config: Config = {
  ApiHost: host,
  ApiBaseUrl: apiBaseUrl,
  Protocol: protocol,
  DiscordAvatarBaseUrl: "https://cdn.discordapp.com/avatars/",
  DiscordGuildIconBaseUrl: "https://cdn.discordapp.com/icons/",
}

export default config
