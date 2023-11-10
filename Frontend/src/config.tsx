type Config = {
  ApiBaseUrl: string,
  DiscordAvatarBaseUrl: string,
  DiscordGuildIconBaseUrl: string
}

let config: Config = {
  ApiBaseUrl: "http://localhost:3000/api/",
  DiscordAvatarBaseUrl: "https://cdn.discordapp.com/avatars/",
  DiscordGuildIconBaseUrl: "https://cdn.discordapp.com/icons/"
}

export default config;