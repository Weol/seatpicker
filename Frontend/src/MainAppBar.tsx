import MenuIcon from "@mui/icons-material/Menu"
import AppBar from "@mui/material/AppBar"
import Box from "@mui/material/Box"
import Button from "@mui/material/Button"
import Container from "@mui/material/Container"
import IconButton from "@mui/material/IconButton"
import Menu from "@mui/material/Menu"
import MenuItem from "@mui/material/MenuItem"
import Toolbar from "@mui/material/Toolbar"
import Tooltip from "@mui/material/Tooltip"
import Typography from "@mui/material/Typography"
import * as React from "react"
import { useNavigate } from "react-router-dom"
import { useAuth } from "./Adapters/AuthAdapter"
import { ActiveGuild, Role } from "./Adapters/Models"
import { RedirectToDiscordLogin } from "./Adapters/RedirectToDiscordLogin"
import { DiscordUserAvatar } from "./Components/DiscordAvatar"

// eslint-disable-next-line @typescript-eslint/no-var-requires
const discordIcon = require("./Media/discord.svg").default

const settings = ["Logg ut"]

export default function MainAppBar(props: { activeGuild: ActiveGuild | null }) {
  const { logout, loggedInUser } = useAuth()
  const [anchorElNav, setAnchorElNav] = React.useState<null | HTMLElement>(null)
  const [anchorElUser, setAnchorElUser] = React.useState<null | HTMLElement>(null)
  const navigate = useNavigate()

  const getPages = () => {
    if (loggedInUser != null) {
      const options = [] as string[]
      options.push("Reserver plass")
      if (loggedInUser.roles.includes(Role.ADMIN) && props.activeGuild) options.push("Admin")
      if (loggedInUser.roles.includes(Role.SUPERADMIN)) options.push("Superadmin")
      return options
    }
    return []
  }

  const handleOpenNavMenu = (event: React.MouseEvent<HTMLElement>) => {
    setAnchorElNav(event.currentTarget)
  }
  const handleOpenUserMenu = (event: React.MouseEvent<HTMLElement>) => {
    setAnchorElUser(event.currentTarget)
  }

  const handleCloseNavMenu = (page: string) => {
    setAnchorElNav(null)

    if (page == "Admin" && props.activeGuild) {
      navigate(`/guild/${props.activeGuild.guildId}`)
    } else if (page == "Reserver plass") {
      navigate(`/reserve`)
    } else if (page == "Superadmin") {
      navigate(`/guilds`)
    }
  }

  const handleCloseUserMenu = (setting: string) => {
    setAnchorElUser(null)

    if (setting == "Logg ut") {
      logout()
      navigate("/")
    }
  }

  return <AppBar position="static">
    <Container maxWidth="xl">
      <Toolbar disableGutters>
        <Typography
          variant="h6"
          noWrap
          component="a"
          onClick={() => navigate("/")}
          sx={{
            mr: 2,
            display: { xs: "none", md: "flex" },
            fontFamily: "monospace",
            fontWeight: 700,
            letterSpacing: ".3rem",
            color: "inherit",
            textDecoration: "none",
            cursor: "pointer",
          }}
        >SALTENLAN</Typography>

        <Box sx={{ flexGrow: 1, display: { xs: "flex", md: "none" } }}>
          <IconButton
            size="large"
            aria-label="account of current user"
            aria-controls="menu-appbar"
            aria-haspopup="true"
            onClick={handleOpenNavMenu}
            color="inherit"
          >
            <MenuIcon />
          </IconButton>
          <Menu
            id="menu-appbar"
            anchorEl={anchorElNav}
            anchorOrigin={{
              vertical: "bottom",
              horizontal: "left",
            }}
            keepMounted
            transformOrigin={{
              vertical: "top",
              horizontal: "left",
            }}
            open={Boolean(anchorElNav)}
            onClose={handleCloseNavMenu}
            sx={{
              display: { xs: "block", md: "none" },
            }}
          >
            {getPages().map(page => <MenuItem
              key={page}
              onClick={() => {
                handleCloseNavMenu(page)
              }}
            >
              <Typography textAlign="center">{page}</Typography>
            </MenuItem>)}
          </Menu>
        </Box>
        <Typography
          variant="h5"
          noWrap
          component="a"
          onClick={() => navigate("/")}
          sx={{
            mr: 2,
            display: { xs: "flex", md: "none" },
            flexGrow: 1,
            fontFamily: "monospace",
            fontWeight: 700,
            letterSpacing: ".3rem",
            color: "inherit",
            textDecoration: "none",
            cursor: "pointer",
          }}
        >
          SALTENLAN
        </Typography>
        <Box sx={{ flexGrow: 1, display: { xs: "none", md: "flex" } }}>
          {getPages().map(page => <Button
            key={page}
            onClick={() => {
              handleCloseNavMenu(page)
            }}
            sx={{ my: 2, color: "white", display: "block" }}
          >
            {page}
          </Button>)}
        </Box>

        {loggedInUser && <Box sx={{ flexGrow: 0 }}>
            <Tooltip title="Open settings">
              <IconButton onClick={handleOpenUserMenu} sx={{ p: 0 }}>
                <DiscordUserAvatar user={loggedInUser} />
              </IconButton>
            </Tooltip>
            <Menu
              sx={{ mt: "45px" }}
              id="menu-appbar"
              anchorEl={anchorElUser}
              anchorOrigin={{
                vertical: "top",
                horizontal: "right",
              }}
              keepMounted
              transformOrigin={{
                vertical: "top",
                horizontal: "right",
              }}
              open={Boolean(anchorElUser)}
              onClose={handleCloseUserMenu}
            >
              {settings.map(setting => <MenuItem
                key={setting}
                onClick={() => {
                  handleCloseUserMenu(setting)
                }}
              >
                <Typography textAlign="center">{setting}</Typography>
              </MenuItem>)}
            </Menu>
          </Box> || <Box sx={{ flexGrow: 0 }}>
            <Button
              sx={{ color: "text.primary" }}
              startIcon={<img src={discordIcon} style={{ width: 20 }} alt="avatar" />}
              variant="text"
              onClick={RedirectToDiscordLogin}
            >
              Logg inn
            </Button>
          </Box>}
      </Toolbar>
    </Container>
  </AppBar>
}
