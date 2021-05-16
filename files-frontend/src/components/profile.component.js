import React, { Component } from "react";
import AuthService from "../services/auth.service";

export default class Profile extends Component {
  constructor(props) {
    super(props);

    this.state = {
      currentUser: AuthService.getCurrentUser()
    };
  }

  render() {
    const { currentUser } = this.state;

    return (
      <div className="container">
        <header className="jumbotron">
          <h3>
            <strong>{currentUser.username}</strong> Profile
          </h3>
        </header>
        <p>
          <strong>First Name:</strong>{" "}
          {currentUser.firstName}
        </p>
        <p>
          <strong>Last Name:</strong>{" "}
          {currentUser.lastName}
        </p>
      </div>
    );
  }
}
