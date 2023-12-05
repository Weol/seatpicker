type Config = {
  ApiHost: string
  DiscordAvatarBaseUrl: string
  DiscordGuildIconBaseUrl: string
}

const config: Config = {
  ApiHost: window.location.host,
  DiscordAvatarBaseUrl: "https://cdn.discordapp.com/avatars/",
  DiscordGuildIconBaseUrl: "https://cdn.discordapp.com/icons/",
}

export default config
