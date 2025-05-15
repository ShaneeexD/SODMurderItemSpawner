import { ThemeProvider, createTheme, CssBaseline } from '@mui/material';
import ConfigForm from './components/ConfigForm';

const theme = createTheme({
  palette: {
    mode: 'light',
    primary: {
      main: '#1976d2',
    },
    secondary: {
      main: '#dc004e',
    },
  },
});

function App() {
  return (
    <ThemeProvider theme={theme}>
      <CssBaseline />
      <ConfigForm />
    </ThemeProvider>
  );
}

export default App
