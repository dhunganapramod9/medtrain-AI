import React from 'react';
import { Link } from 'react-router-dom';

const Footer = () => {
  return (
    <footer className="bg-gray-800 text-white mt-12">
      <div className="max-w-7xl mx-auto py-12 px-4 sm:px-6 lg:px-8">
        <div className="grid grid-cols-1 md:grid-cols-4 gap-8">
          <div>
            <h3 className="text-lg font-semibold mb-4">About Us</h3>
            <p className="text-gray-300">Leading healthcare training platform for medical professionals.</p>
          </div>
          <div>
            <h3 className="text-lg font-semibold mb-4">Quick Links</h3>
            <ul className="space-y-2">
              <li><Link to="/modules" className="text-gray-300 hover:text-white">Learning Modules</Link></li>
              <li><Link to="/clinical-skills" className="text-gray-300 hover:text-white">Clinical Skills</Link></li>
              <li><Link to="/assessment" className="text-gray-300 hover:text-white">Assessment</Link></li>
            </ul>
          </div>
          <div>
            <h3 className="text-lg font-semibold mb-4">Resources</h3>
            <ul className="space-y-2">
              <li><Link to="/resources" className="text-gray-300 hover:text-white">Medical Calculator</Link></li>
              <li><Link to="/guidelines" className="text-gray-300 hover:text-white">Guidelines</Link></li>
              <li><Link to="/research" className="text-gray-300 hover:text-white">Research Tools</Link></li>
            </ul>
          </div>
          <div>
            <h3 className="text-lg font-semibold mb-4">Contact</h3>
            <ul className="space-y-2">
              <li className="text-gray-300">support@medtraining.com</li>
              <li className="text-gray-300">1-800-MED-TRAIN</li>
            </ul>
          </div>
        </div>
        <div className="mt-8 pt-8 border-t border-gray-700 text-center">
          <p className="text-gray-300">&copy; 2024 MedTraining. All rights reserved.</p>
        </div>
      </div>
    </footer>
  );
};

export default Footer;