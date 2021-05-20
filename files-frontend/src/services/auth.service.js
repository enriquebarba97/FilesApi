import axios from "axios";

const API_URL = process.env.REACT_APP_API_URL + "user/";

class AuthService {
  login(username, password) {
    return axios
      .post(API_URL + "authenticate", {
        username,
        password
      })
      .then(response => {
        if (response.data.token) {
          localStorage.setItem("user", JSON.stringify(response.data));
        }

        return response.data;
      });
  }

  logout() {
    localStorage.removeItem("user");
  }

  register(firstName, lastName, username, password) {
    return axios.post(API_URL + "register", {
      firstName,
      lastName,
      username,
      password
    });
  }

  getCurrentUser() {
    return JSON.parse(localStorage.getItem('user'));;
  }
}

export default new AuthService();
