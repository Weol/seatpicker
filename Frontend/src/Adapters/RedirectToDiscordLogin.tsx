import config from "../config"

export default function RedirectToDiscordLogin() {
  var redirectUrl = window.location.origin + "/redirect-login"
  var uri = `https://discord.com/api/oauth2/authorize?client_id=1042448593312821248&redirect_uri=${encodeURIComponent(redirectUrl)}&response_type=code&scope=identify%20guilds.join`
  window.location.replace(uri)
}