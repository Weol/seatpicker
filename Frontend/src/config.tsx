type Config = {
  ApiBaseUrl: string,
  DiscordAvatarBaseUrl: string
}

let config: Config = {
  ApiBaseUrl: "http://localhost:3000/api/",
  DiscordAvatarBaseUrl: "https://cdn.discordapp.com/avatars/"
}

export default config;