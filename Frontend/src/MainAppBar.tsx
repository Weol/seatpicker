import * as React from 'react';
import {useState} from 'react';
import AppBar from '@mui/material/AppBar';
import Box from '@mui/material/Box';
import Toolbar from '@mui/material/Toolbar';
import IconButton from '@mui/material/IconButton';
import Typography from '@mui/material/Typography';
import Menu from '@mui/material/Menu';
import MenuIcon from '@mui/icons-material/Menu';
import Container from '@mui/material/Container';
import Avatar from '@mui/material/Avatar';
import Button from '@mui/material/Button';
import MenuItem from '@mui/material/MenuItem';
import Config from './config'
import User from './Models/User';
import {useNavigate} from "react-router-dom";
import discord from "./Media/discord.svg";
import RedirectToDiscordLogin from "./Adapters/RedirectToDiscordLogin";
import {useUserContext} from "./UserContext";
import {AuthenticationAdapter} from "./Adapters/AuthenticationAdapter";

function ResponsiveAppBar() {
  const [anchorElNav, setAnchorElNav] = useState<null | HTMLElement>(null);
  const [anchorElUser, setAnchorElUser] = useState<null | HTMLElement>(null);
  const {user, setUser} = useUserContext()
  const navigate = useNavigate()

  const pages = [
    {
      Title: 'Setevalg',
      Path: '/'
    },
    {
      Title: 'Info',
      Path: '/about'
    }
  ]

  const handleOpenNavMenu = (event: React.MouseEvent<HTMLElement>) => {
    setAnchorElNav(event.currentTarget);

  };
  const handleOpenUserMenu = (event: React.MouseEvent<HTMLElement>) => {
    setAnchorElUser(event.currentTarget);
  };

  const handleCloseNavMenu = () => {
    setAnchorElNav(null);
  };

  const handleCloseUserMenu = () => {
    setAnchorElUser(null);
  };

  const getAvatarUrl = (user: User | null) => {
    return Config.DiscordAvatarBaseUrl + user?.id + "/" + user?.avatar
  }

  const navigateTo = (path: string) => {
    navigate(path)
    handleCloseNavMenu()
  }

  const handleLogout = () => {
    handleCloseUserMenu()

    setUser(null)
  }

  return (
    <AppBar position="static" sx={{bgcolor: 'background.paper', color: 'text.primary'}}>
      <Container maxWidth="xl">
        <Toolbar disableGutters>
          <Typography
            variant="h6"
            noWrap
            component="a"
            href="/"
            sx={{
              mr: 2,
              flexGrow: 1,
              display: 'block',
              fontFamily: 'monospace',
              fontWeight: 700,
              letterSpacing: '.3rem',
              color: 'inherit',
              textDecoration: 'none',
            }}
          >
            SALTENLAN
          </Typography>
          {user && (
            <Box sx={{flexGrow: 0}}>
              <IconButton onClick={handleOpenUserMenu} sx={{p: 0}}>
                <Avatar alt={user.nick} src={getAvatarUrl(user)}/>
              </IconButton>
              <Menu
                sx={{mt: '45px'}}
                id="menu-appbar"
                anchorEl={anchorElUser}
                anchorOrigin={{
                  vertical: 'top',
                  horizontal: 'right',
                }}
                keepMounted
                transformOrigin={{
                  vertical: 'top',
                  horizontal: 'right',
                }}
                open={Boolean(anchorElUser)}
                onClose={handleCloseUserMenu}
              >
                <MenuItem key='logout' onClick={handleLogout}>
                  <Typography textAlign="center">Logg ut</Typography>
                </MenuItem>
              </Menu>
            </Box>
          ) || (
            <Box sx={{flexGrow: 0}}>
              <Button sx={{color: 'text.primary'}}
                      startIcon={<img src={discord} style={{width: 20}} alt="avatar"/>} variant="text"
                      onClick={RedirectToDiscordLogin}>Logg inn</Button>
            </Box>
          )}
        </Toolbar>
      </Container>
    </AppBar>
  );
}

export default ResponsiveAppBar;