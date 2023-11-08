import * as React from 'react';
import Divider from '@mui/material/Divider';
import Paper from '@mui/material/Paper';
import MenuList from '@mui/material/MenuList';
import MenuItem from '@mui/material/MenuItem';
import ListItemText from '@mui/material/ListItemText';
import ListItemIcon from '@mui/material/ListItemIcon';
import ContentCut from '@mui/icons-material/ContentCut';
import ContentCopy from '@mui/icons-material/ContentCopy';
import ContentPaste from '@mui/icons-material/ContentPaste';
import Cloud from '@mui/icons-material/Cloud';
import Seat from "../Models/Seat";
import {Add, Clear} from "@mui/icons-material";

export function AdminSeatMenu(seat: Seat) {
  const addReservation = () => {
    return (
      <MenuItem>
        <ListItemIcon>
          <ContentCut fontSize="small"/>
        </ListItemIcon>
        <ListItemText>Legg til reservasjon</ListItemText>
      </MenuItem>
    )
  }

  const moveReservation = () => {
    return (
      <MenuItem>
        <ListItemIcon>
          <ContentCut fontSize="small"/>
        </ListItemIcon>
        <ListItemText>Legg til reservasjon</ListItemText>
      </MenuItem>
    )
  }

  const deleteReservation = () => {
    return (
      <MenuItem>
        <ListItemIcon>
          <ContentCut fontSize="small"/>
        </ListItemIcon>
        <ListItemText>Legg til reservasjon</ListItemText>
      </MenuItem>
    )
  }

  const makeReservation = () => {
    return (
      <MenuItem>
        <ListItemIcon>
          <ContentCut fontSize="small"/>
        </ListItemIcon>
        <ListItemText>Legg til reservasjon</ListItemText>
      </MenuItem>
    )
  }

  const deleteSeat = () => {
    return (
      <MenuItem>
        <ListItemIcon>
          <ContentCut fontSize="small"/>
        </ListItemIcon>
        <ListItemText>Legg til reservasjon</ListItemText>
      </MenuItem>
    )
  }

  return (
    <Paper sx={{ width: 320, maxWidth: '100%' }}>
      <MenuList>
        <Divider/>
        <MenuItem>
          <ListItemIcon>
            <Cloud fontSize="small"/>
          </ListItemIcon>
          <ListItemText>Reserver</ListItemText>
        </MenuItem>
      </MenuList>
    </Paper>
  );
}