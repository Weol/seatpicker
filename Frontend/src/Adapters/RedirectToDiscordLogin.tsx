import Config from "../config"

export const RedirectUrl = `${Config.Protocol}://${Config.ApiHost}/redirect-login`

export function RedirectToDiscordLogin() {
  const uri = `https://discord.com/api/oauth2/authorize?client_id=1042448593312821248&redirect_uri=${encodeURIComponent(
    RedirectUrl
  )}&response_type=code&scope=identify%20guilds.join`
  window.location.replace(uri)
}
