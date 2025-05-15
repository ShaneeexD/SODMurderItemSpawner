import React from 'react';
import { 
  AppBar, 
  Toolbar, 
  Typography, 
  Button, 
  Box 
} from '@mui/material';
import { Link as RouterLink, useLocation } from 'react-router-dom';
import HomeIcon from '@mui/icons-material/Home';
import HelpIcon from '@mui/icons-material/Help';

const Navigation: React.FC = () => {
  const location = useLocation();
  
  return (
    <AppBar position="static" color="primary" sx={{ mb: 4 }}>
      <Toolbar>
        <Typography variant="h6" component="div" sx={{ flexGrow: 1 }}>
          MurderItemSpawner Config
        </Typography>
        <Box>
          <Button 
            component={RouterLink} 
            to="/" 
            color="inherit" 
            startIcon={<HomeIcon />}
            sx={{ 
              fontWeight: location.pathname === '/' ? 'bold' : 'normal',
              borderBottom: location.pathname === '/' ? '2px solid white' : 'none'
            }}
          >
            Config Editor
          </Button>
          <Button 
            component={RouterLink} 
            to="/tutorial" 
            color="inherit" 
            startIcon={<HelpIcon />}
            sx={{ 
              fontWeight: location.pathname === '/tutorial' ? 'bold' : 'normal',
              borderBottom: location.pathname === '/tutorial' ? '2px solid white' : 'none'
            }}
          >
            Tutorial
          </Button>
        </Box>
      </Toolbar>
    </AppBar>
  );
};

export default Navigation;
